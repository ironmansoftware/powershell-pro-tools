import * as vscode from 'vscode';
import { PowerShellService } from "./services/powershellservice";
import { commands, ConfigurationChangeEvent, Disposable, ExtensionContext, Uri } from 'vscode';
import { ICommand } from "./commands/command";
import { AstCommands } from "./commands/ast";
import { QuickScriptService } from "./services/quickScriptService";
import { QuickScriptCommands } from "./commands/quickScripts";
import { VariableCommands } from "./commands/variables";
import VSCodeService from "./services/vscodeService";
import { RapidSenseCommand } from './services/rapidsense';
import { ProviderCommands } from './commands/providers';
import { DebuggingCommands } from './commands/debugging';
import { RefactoringCommands } from './commands/refactoring';
import { PowerShellHoverProvider } from './commands/hoverProvider';
import { PowerShellCodeLensProvider } from './commands/codeLensProvider';
import { load } from './settings';
import { signOnSave } from './commands/signOnSave';
import { ReflectionCommands } from './commands/reflection';
import { CustomTreeViewCommands } from './commands/customTreeView';
import { HistoryTreeViewCommands } from './commands/history';
import { SessionCommands } from './commands/sessions';
import { JobTreeViewCommands } from './commands/jobs';
import { PerformanceService } from './services/performanceService';

export class Container {
	static initialize(context: ExtensionContext, powershellService: PowerShellService) {
		this._context = context;
		this._service = powershellService;
		this._commands = [];

		this._commands.push(new AstCommands());
		this._commands.push(new QuickScriptCommands());
		this._commands.push(new VariableCommands());
		this._commands.push(new RapidSenseCommand());
		this._commands.push(new ProviderCommands());
		this._commands.push(new DebuggingCommands());
		this._commands.push(new RefactoringCommands());
		this._commands.push(new ReflectionCommands());
		this._commands.push(new CustomTreeViewCommands());
		this._commands.push(new HistoryTreeViewCommands());
		this._commands.push(new SessionCommands());
		this._commands.push(new JobTreeViewCommands());
		this.RegisterCommands();

		//this._codeLensProvider = new PowerShellCodeLensProvider();

		// let docSelector = {
		// 	language: 'powershell',
		// 	scheme: 'file',
		// }

		// let codeLensProviderDisposable = vscode.languages.registerCodeLensProvider(
		// 	docSelector,
		// 	this._codeLensProvider
		// )

		// context.subscriptions.push(codeLensProviderDisposable)

		this.outputChannel = vscode.window.createOutputChannel("PowerShell Pro Tools");

		this.Log("Starting PowerShell Pro Tools...");
	}

	static FinishInitialize() {
		if (this._initialized) { return; }
		this._quickScriptService = new QuickScriptService();
		this._quickScriptService.initialize();
		signOnSave();
		this._vscodeService = new VSCodeService();
		this._hoverProvider = new PowerShellHoverProvider();
		this._performanceService = new PerformanceService(this._context);
		this._initialized = true;
	}

	private static _initialized: boolean = false;

	private static _commands: ICommand[];
	private static outputChannel: vscode.OutputChannel;

	private static _context: ExtensionContext;
	static get context() {
		return this._context;
	}

	private static _performanceService: PerformanceService;
	static get performanceService() {
		return this._performanceService;
	}


	private static _codeLensProvider: PowerShellCodeLensProvider;

	private static _hoverProvider: PowerShellHoverProvider;
	static get hoverProvider() {
		return this._hoverProvider;
	}

	private static _service: PowerShellService;
	static get PowerShellService() {
		return this._service;
	}

	private static _quickScriptService: QuickScriptService;
	static get QuickScriptService() {
		return this._quickScriptService;
	}

	static RegisterCommands() {
		this._commands.forEach(x => x.register(this._context));
	}

	private static _vscodeService: VSCodeService;
	static get VSCodeService() {
		return this._vscodeService;
	}

	static Log(msg: string) {
		this.outputChannel.appendLine(msg);
		console.log(msg);
	}

	static FocusLog() {
		this.outputChannel.show();
	}

	static GetSettings() {
		return load();
	}

	static IsInitialized() {
		if (!this._initialized) {
			vscode.window.showWarningMessage("PowerShell Pro Tools is still starting.");
		}

		return this._initialized;
	}
}