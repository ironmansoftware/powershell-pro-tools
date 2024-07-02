import { Container } from "../container";

export class QuickScriptService {

    async initialize() {
        var scripts = Container.context.globalState.get("quick-scripts");
        if (!scripts) {
            await Container.context.globalState.update("quick-scripts", []);
        }
    }

    getScripts() : any[] {
        return Container.context.globalState.get("quick-scripts") as any[];
    }

    getScript(name : string) : any {
        var scripts = Container.context.globalState.get("quick-scripts") as any[];
        return scripts.find(m => m.Name === name);
    }

    async removeScript(name : string) {
        var scripts = Container.context.globalState.get("quick-scripts") as any[];
        scripts = scripts.filter(x => x.Name !== name);
        await Container.context.globalState.update("quick-scripts", scripts);
    }

    async setScript(name : string, fileName : string) {
        var scripts = Container.context.globalState.get("quick-scripts") as any[];

        var script = scripts.find(x => x === name);
        if (script) return;

        scripts.push({
            Name: name, 
            File: fileName
        });
        await Container.context.globalState.update("quick-scripts", scripts);
    } 
}