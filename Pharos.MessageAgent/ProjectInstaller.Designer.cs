﻿namespace Pharos.MessageAgent
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.MessageAgentServerInstaller = new System.ServiceProcess.ServiceInstaller();
            this.ProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            // 
            // MessageAgentServerInstaller
            // 
            this.MessageAgentServerInstaller.Description = "消息代理中间件服务";
            this.MessageAgentServerInstaller.DisplayName = "Pharos.MessageAgentServer";
            this.MessageAgentServerInstaller.ServiceName = "Pharos.MessageAgentServer";
            this.MessageAgentServerInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProcessInstaller
            // 
            this.ProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.ProcessInstaller.Password = null;
            this.ProcessInstaller.Username = null;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.MessageAgentServerInstaller,
            this.ProcessInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceInstaller MessageAgentServerInstaller;
        private System.ServiceProcess.ServiceProcessInstaller ProcessInstaller;
    }
}