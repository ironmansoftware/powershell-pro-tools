using System;
using System.ComponentModel.Design;

namespace IM.WinForms
{
	internal class MegaDesignerTransaction : DesignerTransaction
	{
		private DesignerHost host = null;

		protected override void OnCommit()
		{
			host.OnTransactionClosing(true);
			host.OnTransactionClosed(true);
		}

		protected override void OnCancel()
		{
			host.OnTransactionClosing(false);
			host.OnTransactionClosed(false);
		}

		public MegaDesignerTransaction(DesignerHost host) : base()
		{
			this.host = host;
		}

		public MegaDesignerTransaction(DesignerHost host, string description) : base(description)
		{
			this.host = host;
		}

	}
}
