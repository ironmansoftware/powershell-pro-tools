import * as vscode from 'vscode';
import { Container } from '../container';
import { vscodeApiMetadata } from '../generated/vscodeApiMetadata';

type Operation = 'get' | 'invoke' | 'construct' | 'release';

interface ApiTarget {
    path?: string[];
    handle?: number;
    typeName?: string;
}

interface ApiArgument {
    kind: 'null' | 'primitive' | 'target' | 'value';
    value?: any;
    handle?: number;
    path?: string[];
}

interface ApiRequest {
    protocol: 'vscode-api';
    id: string;
    operation: Operation;
    target?: ApiTarget;
    member?: string;
    arguments?: ApiArgument[];
}

interface ApiResult {
    kind: 'primitive' | 'array' | 'handle' | 'null' | 'undefined';
    value?: any;
    handle?: number;
    typeName?: string;
    items?: ApiResult[];
}

interface ApiResponse {
    id: string;
    success: boolean;
    error?: string;
    result?: ApiResult;
}

export class VSCodeApiDispatcher {
    private handles: Map<number, any> = new Map<number, any>();
    private nextHandle = 1;

    async dispatch(request: ApiRequest): Promise<ApiResponse> {
        try {
            Container.Log(`VS Code API ${request.operation}: ${this.describeTarget(request.target)}${request.member ? `.${request.member}` : ''}`);
            const result = await this.execute(request);
            const serialized = await this.serializeResult(result);
            Container.Log(`VS Code API result: ${serialized.kind}${serialized.typeName ? ` ${serialized.typeName}` : ''}${serialized.value !== undefined ? ` ${serialized.value}` : ''}`);
            return this.createResponse(request.id, true, serialized);
        }
        catch (error) {
            const message = error instanceof Error ? error.message : `${error}`;
            Container.Log(`VS Code API error: ${error instanceof Error ? error.stack || error.message : error}`);
            return this.createResponse(request.id, false, undefined, message);
        }
    }

    private createResponse(id: string, success: boolean, result?: ApiResult, error?: string): ApiResponse {
        const response: any = {
            id,
            success,
            error,
            result,
            Id: id,
            Success: success,
            Error: error,
            Result: this.createPascalResult(result)
        };

        return response;
    }

    private createPascalResult(result?: ApiResult): any {
        if (!result) {
            return undefined;
        }

        return {
            kind: result.kind,
            value: result.value,
            handle: result.handle,
            typeName: result.typeName,
            items: result.items,
            Kind: result.kind,
            Value: result.value,
            Handle: result.handle,
            TypeName: result.typeName,
            Items: result.items ? result.items.map(item => this.createPascalResult(item)) : undefined
        };
    }

    private async execute(request: ApiRequest): Promise<any> {
        if (request.operation === 'release') {
            if (request.target && request.target.handle) {
                this.handles.delete(request.target.handle);
            }

            return null;
        }

        const target = this.resolveTarget(request.target);
        const args = (request.arguments || []).map(arg => this.deserializeArgument(arg));

        switch (request.operation) {
            case 'get':
                return request.member ? target[request.member] : target;
            case 'invoke':
                if (!request.member) {
                    throw new Error('Invoke requests require a member name.');
                }

                if (!this.isKnownMember(request.target, request.member)) {
                    throw new Error(`VS Code API member is not in the generated surface: ${request.member}`);
                }

                if (typeof target[request.member] !== 'function') {
                    throw new Error(`VS Code API member is not invokable: ${request.member}`);
                }

                return target[request.member](...args);
            case 'construct':
                if (typeof target !== 'function') {
                    throw new Error('Construct target is not a constructor.');
                }

                return new target(...args);
            default:
                throw new Error(`Unsupported VS Code API operation: ${request.operation}`);
        }
    }

    private resolveTarget(target?: ApiTarget): any {
        if (!target) {
            return vscode;
        }

        if (target.handle) {
            if (!this.handles.has(target.handle)) {
                throw new Error(`Unknown VS Code API object handle: ${target.handle}`);
            }

            return this.handles.get(target.handle);
        }

        let current: any = vscode;
        for (const segment of target.path || []) {
            if (current == null || !(segment in current)) {
                throw new Error(`VS Code API path was not found: vscode.${(target.path || []).join('.')}`);
            }

            current = current[segment];
        }

        return current;
    }

    private deserializeArgument(argument: ApiArgument): any {
        if (!argument || argument.kind === 'null') {
            return null;
        }

        if (argument.kind === 'target') {
            return this.resolveTarget({
                handle: argument.handle,
                path: argument.path
            });
        }

        return argument.value;
    }

    private async serializeResult(value: any): Promise<ApiResult> {
        value = await this.resolveThenable(value);

        if (value === undefined) {
            return { kind: 'undefined' };
        }

        if (value === null) {
            return { kind: 'null' };
        }

        if (Array.isArray(value)) {
            const items = [];
            for (const item of value) {
                items.push(await this.serializeResult(item));
            }

            return { kind: 'array', items };
        }

        const valueType = typeof value;
        if (valueType === 'string' || valueType === 'number' || valueType === 'boolean') {
            return { kind: 'primitive', value };
        }

        return {
            kind: 'handle',
            handle: this.createHandle(value),
            typeName: this.getTypeName(value)
        };
    }

    private async resolveThenable(value: any): Promise<any> {
        if (value && typeof value.then === 'function') {
            return await value;
        }

        return value;
    }

    private createHandle(value: any): number {
        const handle = this.nextHandle++;
        this.handles.set(handle, value);
        return handle;
    }

    private getTypeName(value: any): string {
        if (value && value.constructor && value.constructor.name) {
            return value.constructor.name;
        }

        return 'Object';
    }

    private isKnownMember(target: ApiTarget, member: string): boolean {
        if (!target || target.handle) {
            return true;
        }

        const path = target.path || [];
        const key = path.join('.');
        const members = vscodeApiMetadata.members[key];
        return !members || members.indexOf(member) !== -1;
    }

    private describeTarget(target?: ApiTarget): string {
        if (!target) {
            return 'vscode';
        }

        if (target.handle) {
            return `handle:${target.handle}`;
        }

        return `vscode.${(target.path || []).join('.')}`;
    }
}
