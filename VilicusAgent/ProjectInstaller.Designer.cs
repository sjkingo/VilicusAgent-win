namespace VilicusAgent
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.VilicusAgentProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.VilicusAgentInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // VilicusAgentProcessInstaller
            // 
            this.VilicusAgentProcessInstaller.Account = System.ServiceProcess.ServiceAccount.NetworkService;
            this.VilicusAgentProcessInstaller.Password = null;
            this.VilicusAgentProcessInstaller.Username = null;
            // 
            // VilicusAgentInstaller
            // 
            this.VilicusAgentInstaller.Description = "Agent for the Vilicus monitoring system";
            this.VilicusAgentInstaller.DisplayName = "Vilicus Agent";
            this.VilicusAgentInstaller.ServiceName = "VilicusAgent";
            this.VilicusAgentInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.VilicusAgentProcessInstaller,
            this.VilicusAgentInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller VilicusAgentProcessInstaller;
        private System.ServiceProcess.ServiceInstaller VilicusAgentInstaller;
    }
}