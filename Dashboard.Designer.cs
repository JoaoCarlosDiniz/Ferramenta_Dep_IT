namespace Ferramenta_IT
{
    partial class Dashboard
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Dashboard));
            barraMenu = new MenuStrip();
            dominioToolStripMenuItem = new ToolStripMenuItem();
            testeDeDominioToolStripMenuItem = new ToolStripMenuItem();
            sincronizaçãoDeTempoToolStripMenuItem = new ToolStripMenuItem();
            windowsToolStripMenuItem = new ToolStripMenuItem();
            testeDeDiscoEWindowsToolStripMenuItem = new ToolStripMenuItem();
            correcãoDoErroRDCCredSSPToolStripMenuItem = new ToolStripMenuItem();
            diagnósticoDeMemóriaToolStripMenuItem = new ToolStripMenuItem();
            backupDeDriversToolStripMenuItem = new ToolStripMenuItem();
            pontoDeRestauroToolStripMenuItem = new ToolStripMenuItem();
            manutençãoDoWindowsToolStripMenuItem = new ToolStripMenuItem();
            utilizadorToolStripMenuItem = new ToolStripMenuItem();
            actualizarComWingetToolStripMenuItem = new ToolStripMenuItem();
            desinstalarComWingetToolStripMenuItem = new ToolStripMenuItem();
            reiniciarServiçoDeImpressãoToolStripMenuItem = new ToolStripMenuItem();
            redeToolStripMenuItem = new ToolStripMenuItem();
            testeDeComunicaçãoEAcessosÀRedeToolStripMenuItem = new ToolStripMenuItem();
            correcçãoDoAdminToolStripMenuItem = new ToolStripMenuItem();
            opçõesToolStripMenuItem = new ToolStripMenuItem();
            actualizarToolStripMenuItem = new ToolStripMenuItem();
            ckEstilo = new CheckBox();
            InfoPC = new GroupBox();
            txWINVersao = new Label();
            txWINEdicao = new Label();
            txProcessador = new Label();
            txNomePC = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            InfoMemoria = new GroupBox();
            lbMemLivre = new Label();
            lbMemUso = new Label();
            lbMemTotal = new Label();
            label8 = new Label();
            label9 = new Label();
            label10 = new Label();
            InfoDisco = new GroupBox();
            lbDiskLivre = new Label();
            lbDiskUso = new Label();
            lbDiskTotal = new Label();
            label14 = new Label();
            label15 = new Label();
            label16 = new Label();
            InfoRede = new GroupBox();
            lbRedeGateway = new Label();
            lbRedeMask = new Label();
            lbRedeIP = new Label();
            label11 = new Label();
            label12 = new Label();
            label13 = new Label();
            hostToolStripMenuItem = new ToolStripMenuItem();
            desbloquearToolStripMenuItem = new ToolStripMenuItem();
            bloqueioTotalToolStripMenuItem = new ToolStripMenuItem();
            bloqueioTotalExceptoOGmailToolStripMenuItem = new ToolStripMenuItem();
            bloqueioRedesSociaisToolStripMenuItem = new ToolStripMenuItem();
            bloqueioStreamToolStripMenuItem = new ToolStripMenuItem();
            bloqueioRedesSociaisEStreamToolStripMenuItem = new ToolStripMenuItem();
            barraMenu.SuspendLayout();
            InfoPC.SuspendLayout();
            InfoMemoria.SuspendLayout();
            InfoDisco.SuspendLayout();
            InfoRede.SuspendLayout();
            SuspendLayout();
            // 
            // barraMenu
            // 
            barraMenu.Font = new Font("Calibri", 9F);
            barraMenu.Items.AddRange(new ToolStripItem[] { dominioToolStripMenuItem, windowsToolStripMenuItem, utilizadorToolStripMenuItem, redeToolStripMenuItem, opçõesToolStripMenuItem });
            barraMenu.Location = new Point(0, 0);
            barraMenu.Name = "barraMenu";
            barraMenu.Size = new Size(800, 24);
            barraMenu.TabIndex = 0;
            barraMenu.Text = "menuStrip1";
            // 
            // dominioToolStripMenuItem
            // 
            dominioToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { testeDeDominioToolStripMenuItem, sincronizaçãoDeTempoToolStripMenuItem });
            dominioToolStripMenuItem.Name = "dominioToolStripMenuItem";
            dominioToolStripMenuItem.Size = new Size(66, 20);
            dominioToolStripMenuItem.Text = "Dominio";
            // 
            // testeDeDominioToolStripMenuItem
            // 
            testeDeDominioToolStripMenuItem.Name = "testeDeDominioToolStripMenuItem";
            testeDeDominioToolStripMenuItem.Size = new Size(205, 22);
            testeDeDominioToolStripMenuItem.Text = "Teste de Dominio";
            testeDeDominioToolStripMenuItem.Click += testeDeDominioToolStripMenuItem_Click;
            // 
            // sincronizaçãoDeTempoToolStripMenuItem
            // 
            sincronizaçãoDeTempoToolStripMenuItem.Name = "sincronizaçãoDeTempoToolStripMenuItem";
            sincronizaçãoDeTempoToolStripMenuItem.Size = new Size(205, 22);
            sincronizaçãoDeTempoToolStripMenuItem.Text = "Sincronização de Tempo";
            sincronizaçãoDeTempoToolStripMenuItem.Click += sincronizaçãoDeTempoToolStripMenuItem_Click;
            // 
            // windowsToolStripMenuItem
            // 
            windowsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { testeDeDiscoEWindowsToolStripMenuItem, correcãoDoErroRDCCredSSPToolStripMenuItem, diagnósticoDeMemóriaToolStripMenuItem, backupDeDriversToolStripMenuItem, pontoDeRestauroToolStripMenuItem, manutençãoDoWindowsToolStripMenuItem });
            windowsToolStripMenuItem.Name = "windowsToolStripMenuItem";
            windowsToolStripMenuItem.Size = new Size(70, 20);
            windowsToolStripMenuItem.Text = "Windows";
            // 
            // testeDeDiscoEWindowsToolStripMenuItem
            // 
            testeDeDiscoEWindowsToolStripMenuItem.Name = "testeDeDiscoEWindowsToolStripMenuItem";
            testeDeDiscoEWindowsToolStripMenuItem.Size = new Size(236, 22);
            testeDeDiscoEWindowsToolStripMenuItem.Text = "Teste de Disco e Windows";
            testeDeDiscoEWindowsToolStripMenuItem.Click += testeDeDiscoEWindowsToolStripMenuItem_Click;
            // 
            // correcãoDoErroRDCCredSSPToolStripMenuItem
            // 
            correcãoDoErroRDCCredSSPToolStripMenuItem.Name = "correcãoDoErroRDCCredSSPToolStripMenuItem";
            correcãoDoErroRDCCredSSPToolStripMenuItem.Size = new Size(236, 22);
            correcãoDoErroRDCCredSSPToolStripMenuItem.Text = "Correcção do Erro RDC CredSSP";
            correcãoDoErroRDCCredSSPToolStripMenuItem.Click += correcãoDoErroRDCCredSSPToolStripMenuItem_Click;
            // 
            // diagnósticoDeMemóriaToolStripMenuItem
            // 
            diagnósticoDeMemóriaToolStripMenuItem.Name = "diagnósticoDeMemóriaToolStripMenuItem";
            diagnósticoDeMemóriaToolStripMenuItem.Size = new Size(236, 22);
            diagnósticoDeMemóriaToolStripMenuItem.Text = "Diagnóstico de Memória";
            diagnósticoDeMemóriaToolStripMenuItem.Click += diagnósticoDeMemóriaToolStripMenuItem_Click;
            // 
            // backupDeDriversToolStripMenuItem
            // 
            backupDeDriversToolStripMenuItem.Name = "backupDeDriversToolStripMenuItem";
            backupDeDriversToolStripMenuItem.Size = new Size(236, 22);
            backupDeDriversToolStripMenuItem.Text = "Backup de Drivers";
            backupDeDriversToolStripMenuItem.Click += backupDeDriversToolStripMenuItem_Click;
            // 
            // pontoDeRestauroToolStripMenuItem
            // 
            pontoDeRestauroToolStripMenuItem.Name = "pontoDeRestauroToolStripMenuItem";
            pontoDeRestauroToolStripMenuItem.Size = new Size(236, 22);
            pontoDeRestauroToolStripMenuItem.Text = "Ponto de Restauro";
            pontoDeRestauroToolStripMenuItem.Click += pontoDeRestauroToolStripMenuItem_Click;
            // 
            // manutençãoDoWindowsToolStripMenuItem
            // 
            manutençãoDoWindowsToolStripMenuItem.Name = "manutençãoDoWindowsToolStripMenuItem";
            manutençãoDoWindowsToolStripMenuItem.Size = new Size(236, 22);
            manutençãoDoWindowsToolStripMenuItem.Text = "Manutenção do Windows";
            manutençãoDoWindowsToolStripMenuItem.Click += manutençãoDoWindowsToolStripMenuItem_Click;
            // 
            // utilizadorToolStripMenuItem
            // 
            utilizadorToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { actualizarComWingetToolStripMenuItem, desinstalarComWingetToolStripMenuItem, reiniciarServiçoDeImpressãoToolStripMenuItem });
            utilizadorToolStripMenuItem.Name = "utilizadorToolStripMenuItem";
            utilizadorToolStripMenuItem.Size = new Size(72, 20);
            utilizadorToolStripMenuItem.Text = "Utilizador";
            // 
            // actualizarComWingetToolStripMenuItem
            // 
            actualizarComWingetToolStripMenuItem.Name = "actualizarComWingetToolStripMenuItem";
            actualizarComWingetToolStripMenuItem.Size = new Size(242, 22);
            actualizarComWingetToolStripMenuItem.Text = "Actualizar com Winget";
            actualizarComWingetToolStripMenuItem.Click += actualizarComWingetToolStripMenuItem_Click;
            // 
            // desinstalarComWingetToolStripMenuItem
            // 
            desinstalarComWingetToolStripMenuItem.Name = "desinstalarComWingetToolStripMenuItem";
            desinstalarComWingetToolStripMenuItem.Size = new Size(242, 22);
            desinstalarComWingetToolStripMenuItem.Text = "Desinstalar com Winget";
            desinstalarComWingetToolStripMenuItem.Click += desinstalarComWingetToolStripMenuItem_Click;
            // 
            // reiniciarServiçoDeImpressãoToolStripMenuItem
            // 
            reiniciarServiçoDeImpressãoToolStripMenuItem.Name = "reiniciarServiçoDeImpressãoToolStripMenuItem";
            reiniciarServiçoDeImpressãoToolStripMenuItem.Size = new Size(242, 22);
            reiniciarServiçoDeImpressãoToolStripMenuItem.Text = "Reiniciar Serviço de Impressão";
            reiniciarServiçoDeImpressãoToolStripMenuItem.Click += reiniciarServiçoDeImpressãoToolStripMenuItem_Click;
            // 
            // redeToolStripMenuItem
            // 
            redeToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { testeDeComunicaçãoEAcessosÀRedeToolStripMenuItem, correcçãoDoAdminToolStripMenuItem, hostToolStripMenuItem });
            redeToolStripMenuItem.Name = "redeToolStripMenuItem";
            redeToolStripMenuItem.Size = new Size(47, 20);
            redeToolStripMenuItem.Text = "Rede";
            // 
            // testeDeComunicaçãoEAcessosÀRedeToolStripMenuItem
            // 
            testeDeComunicaçãoEAcessosÀRedeToolStripMenuItem.Name = "testeDeComunicaçãoEAcessosÀRedeToolStripMenuItem";
            testeDeComunicaçãoEAcessosÀRedeToolStripMenuItem.Size = new Size(293, 22);
            testeDeComunicaçãoEAcessosÀRedeToolStripMenuItem.Text = "Teste de Comunicação e Acessos à Rede";
            testeDeComunicaçãoEAcessosÀRedeToolStripMenuItem.Click += testeDeComunicaçãoEAcessosÀRedeToolStripMenuItem_Click;
            // 
            // correcçãoDoAdminToolStripMenuItem
            // 
            correcçãoDoAdminToolStripMenuItem.Name = "correcçãoDoAdminToolStripMenuItem";
            correcçãoDoAdminToolStripMenuItem.Size = new Size(293, 22);
            correcçãoDoAdminToolStripMenuItem.Text = "Correcção do Admin";
            correcçãoDoAdminToolStripMenuItem.Click += correcçãoDoAdminToolStripMenuItem_Click;
            // 
            // opçõesToolStripMenuItem
            // 
            opçõesToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { actualizarToolStripMenuItem });
            opçõesToolStripMenuItem.Name = "opçõesToolStripMenuItem";
            opçõesToolStripMenuItem.Size = new Size(59, 20);
            opçõesToolStripMenuItem.Text = "Opções";
            // 
            // actualizarToolStripMenuItem
            // 
            actualizarToolStripMenuItem.Name = "actualizarToolStripMenuItem";
            actualizarToolStripMenuItem.Size = new Size(128, 22);
            actualizarToolStripMenuItem.Text = "Actualizar";
            actualizarToolStripMenuItem.Click += actualizarToolStripMenuItem_Click;
            // 
            // ckEstilo
            // 
            ckEstilo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ckEstilo.AutoSize = true;
            ckEstilo.BackColor = Color.Transparent;
            ckEstilo.Font = new Font("Calibri", 10F);
            ckEstilo.Location = new Point(691, 27);
            ckEstilo.Name = "ckEstilo";
            ckEstilo.RightToLeft = RightToLeft.Yes;
            ckEstilo.Size = new Size(97, 21);
            ckEstilo.TabIndex = 1;
            ckEstilo.Text = "Estilo Escuro";
            ckEstilo.UseVisualStyleBackColor = false;
            ckEstilo.CheckedChanged += ckEstilo_CheckedChanged;
            // 
            // InfoPC
            // 
            InfoPC.BackColor = Color.Transparent;
            InfoPC.Controls.Add(txWINVersao);
            InfoPC.Controls.Add(txWINEdicao);
            InfoPC.Controls.Add(txProcessador);
            InfoPC.Controls.Add(txNomePC);
            InfoPC.Controls.Add(label4);
            InfoPC.Controls.Add(label3);
            InfoPC.Controls.Add(label2);
            InfoPC.Controls.Add(label1);
            InfoPC.Font = new Font("Calibri", 11F, FontStyle.Bold);
            InfoPC.Location = new Point(12, 27);
            InfoPC.Name = "InfoPC";
            InfoPC.Size = new Size(370, 114);
            InfoPC.TabIndex = 2;
            InfoPC.TabStop = false;
            InfoPC.Text = "Informação da Maquina";
            // 
            // txWINVersao
            // 
            txWINVersao.AutoSize = true;
            txWINVersao.Font = new Font("Calibri", 10F);
            txWINVersao.Location = new Point(88, 79);
            txWINVersao.Name = "txWINVersao";
            txWINVersao.Size = new Size(82, 17);
            txWINVersao.TabIndex = 7;
            txWINVersao.Text = "txWINVersao";
            // 
            // txWINEdicao
            // 
            txWINEdicao.AutoSize = true;
            txWINEdicao.Font = new Font("Calibri", 10F);
            txWINEdicao.Location = new Point(88, 61);
            txWINEdicao.Name = "txWINEdicao";
            txWINEdicao.Size = new Size(81, 17);
            txWINEdicao.TabIndex = 6;
            txWINEdicao.Text = "txWINEdicao";
            // 
            // txProcessador
            // 
            txProcessador.AutoSize = true;
            txProcessador.Font = new Font("Calibri", 10F);
            txProcessador.Location = new Point(88, 43);
            txProcessador.Name = "txProcessador";
            txProcessador.Size = new Size(87, 17);
            txProcessador.TabIndex = 5;
            txProcessador.Text = "txProcessador";
            // 
            // txNomePC
            // 
            txNomePC.AutoSize = true;
            txNomePC.Font = new Font("Calibri", 10F);
            txNomePC.Location = new Point(88, 25);
            txNomePC.Name = "txNomePC";
            txNomePC.Size = new Size(67, 17);
            txNomePC.TabIndex = 4;
            txNomePC.Text = "txNomePC";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Calibri", 10F);
            label4.Location = new Point(6, 79);
            label4.Name = "label4";
            label4.Size = new Size(50, 17);
            label4.TabIndex = 3;
            label4.Text = "Versão:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Calibri", 10F);
            label3.Location = new Point(6, 61);
            label3.Name = "label3";
            label3.Size = new Size(49, 17);
            label3.TabIndex = 2;
            label3.Text = "Edição:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Calibri", 10F);
            label2.Location = new Point(6, 43);
            label2.Name = "label2";
            label2.Size = new Size(80, 17);
            label2.TabIndex = 1;
            label2.Text = "Processador:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Calibri", 10F);
            label1.ForeColor = SystemColors.ControlText;
            label1.Location = new Point(6, 25);
            label1.Name = "label1";
            label1.Size = new Size(46, 17);
            label1.TabIndex = 0;
            label1.Text = "Nome:";
            // 
            // InfoMemoria
            // 
            InfoMemoria.BackColor = Color.Transparent;
            InfoMemoria.Controls.Add(lbMemLivre);
            InfoMemoria.Controls.Add(lbMemUso);
            InfoMemoria.Controls.Add(lbMemTotal);
            InfoMemoria.Controls.Add(label8);
            InfoMemoria.Controls.Add(label9);
            InfoMemoria.Controls.Add(label10);
            InfoMemoria.Font = new Font("Calibri", 11F, FontStyle.Bold);
            InfoMemoria.Location = new Point(12, 147);
            InfoMemoria.Name = "InfoMemoria";
            InfoMemoria.Size = new Size(270, 100);
            InfoMemoria.TabIndex = 3;
            InfoMemoria.TabStop = false;
            InfoMemoria.Text = "Informação da Memória";
            // 
            // lbMemLivre
            // 
            lbMemLivre.AutoSize = true;
            lbMemLivre.Font = new Font("Calibri", 10F);
            lbMemLivre.Location = new Point(90, 59);
            lbMemLivre.Name = "lbMemLivre";
            lbMemLivre.Size = new Size(75, 17);
            lbMemLivre.TabIndex = 12;
            lbMemLivre.Text = "lbMemLivre";
            // 
            // lbMemUso
            // 
            lbMemUso.AutoSize = true;
            lbMemUso.Font = new Font("Calibri", 10F);
            lbMemUso.Location = new Point(90, 41);
            lbMemUso.Name = "lbMemUso";
            lbMemUso.Size = new Size(69, 17);
            lbMemUso.TabIndex = 11;
            lbMemUso.Text = "lbMemUso";
            // 
            // lbMemTotal
            // 
            lbMemTotal.AutoSize = true;
            lbMemTotal.Font = new Font("Calibri", 10F);
            lbMemTotal.Location = new Point(90, 23);
            lbMemTotal.Name = "lbMemTotal";
            lbMemTotal.Size = new Size(76, 17);
            lbMemTotal.TabIndex = 10;
            lbMemTotal.Text = "lbMemTotal";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Calibri", 10F);
            label8.Location = new Point(6, 59);
            label8.Name = "label8";
            label8.Size = new Size(69, 17);
            label8.TabIndex = 9;
            label8.Text = "Disponivel:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Calibri", 10F);
            label9.Location = new Point(6, 41);
            label9.Name = "label9";
            label9.Size = new Size(54, 17);
            label9.TabIndex = 8;
            label9.Text = "Em Uso:";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Calibri", 10F);
            label10.Location = new Point(6, 23);
            label10.Name = "label10";
            label10.Size = new Size(40, 17);
            label10.TabIndex = 7;
            label10.Text = "Total:";
            // 
            // InfoDisco
            // 
            InfoDisco.BackColor = Color.Transparent;
            InfoDisco.Controls.Add(lbDiskLivre);
            InfoDisco.Controls.Add(lbDiskUso);
            InfoDisco.Controls.Add(lbDiskTotal);
            InfoDisco.Controls.Add(label14);
            InfoDisco.Controls.Add(label15);
            InfoDisco.Controls.Add(label16);
            InfoDisco.Font = new Font("Calibri", 11F, FontStyle.Bold);
            InfoDisco.Location = new Point(288, 147);
            InfoDisco.Name = "InfoDisco";
            InfoDisco.Size = new Size(270, 100);
            InfoDisco.TabIndex = 4;
            InfoDisco.TabStop = false;
            InfoDisco.Text = "Informação do Disco C";
            // 
            // lbDiskLivre
            // 
            lbDiskLivre.AutoSize = true;
            lbDiskLivre.Font = new Font("Calibri", 10F);
            lbDiskLivre.Location = new Point(81, 59);
            lbDiskLivre.Name = "lbDiskLivre";
            lbDiskLivre.Size = new Size(68, 17);
            lbDiskLivre.TabIndex = 18;
            lbDiskLivre.Text = "lbDiskLivre";
            // 
            // lbDiskUso
            // 
            lbDiskUso.AutoSize = true;
            lbDiskUso.Font = new Font("Calibri", 10F);
            lbDiskUso.Location = new Point(81, 41);
            lbDiskUso.Name = "lbDiskUso";
            lbDiskUso.Size = new Size(62, 17);
            lbDiskUso.TabIndex = 17;
            lbDiskUso.Text = "lbDiskUso";
            // 
            // lbDiskTotal
            // 
            lbDiskTotal.AutoSize = true;
            lbDiskTotal.Font = new Font("Calibri", 10F);
            lbDiskTotal.Location = new Point(81, 23);
            lbDiskTotal.Name = "lbDiskTotal";
            lbDiskTotal.Size = new Size(69, 17);
            lbDiskTotal.TabIndex = 16;
            lbDiskTotal.Text = "lbDiskTotal";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new Font("Calibri", 10F);
            label14.Location = new Point(6, 59);
            label14.Name = "label14";
            label14.Size = new Size(69, 17);
            label14.TabIndex = 15;
            label14.Text = "Disponivel:";
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Font = new Font("Calibri", 10F);
            label15.Location = new Point(6, 41);
            label15.Name = "label15";
            label15.Size = new Size(54, 17);
            label15.TabIndex = 14;
            label15.Text = "Em Uso:";
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Font = new Font("Calibri", 10F);
            label16.Location = new Point(6, 23);
            label16.Name = "label16";
            label16.Size = new Size(40, 17);
            label16.TabIndex = 13;
            label16.Text = "Total:";
            // 
            // InfoRede
            // 
            InfoRede.BackColor = Color.Transparent;
            InfoRede.Controls.Add(lbRedeGateway);
            InfoRede.Controls.Add(lbRedeMask);
            InfoRede.Controls.Add(lbRedeIP);
            InfoRede.Controls.Add(label11);
            InfoRede.Controls.Add(label12);
            InfoRede.Controls.Add(label13);
            InfoRede.Font = new Font("Calibri", 11F, FontStyle.Bold);
            InfoRede.Location = new Point(388, 27);
            InfoRede.Name = "InfoRede";
            InfoRede.Size = new Size(170, 114);
            InfoRede.TabIndex = 3;
            InfoRede.TabStop = false;
            InfoRede.Text = "Informação de Rede";
            // 
            // lbRedeGateway
            // 
            lbRedeGateway.AutoSize = true;
            lbRedeGateway.Font = new Font("Calibri", 10F);
            lbRedeGateway.Location = new Point(67, 61);
            lbRedeGateway.Name = "lbRedeGateway";
            lbRedeGateway.Size = new Size(98, 17);
            lbRedeGateway.TabIndex = 24;
            lbRedeGateway.Text = "lbRedeGateway";
            // 
            // lbRedeMask
            // 
            lbRedeMask.AutoSize = true;
            lbRedeMask.Font = new Font("Calibri", 10F);
            lbRedeMask.Location = new Point(67, 43);
            lbRedeMask.Name = "lbRedeMask";
            lbRedeMask.Size = new Size(77, 17);
            lbRedeMask.TabIndex = 23;
            lbRedeMask.Text = "lbRedeMask";
            // 
            // lbRedeIP
            // 
            lbRedeIP.AutoSize = true;
            lbRedeIP.Font = new Font("Calibri", 10F);
            lbRedeIP.Location = new Point(67, 25);
            lbRedeIP.Name = "lbRedeIP";
            lbRedeIP.Size = new Size(58, 17);
            lbRedeIP.TabIndex = 22;
            lbRedeIP.Text = "lbRedeIP";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Calibri", 10F);
            label11.Location = new Point(6, 61);
            label11.Name = "label11";
            label11.Size = new Size(63, 17);
            label11.TabIndex = 21;
            label11.Text = "Gateway:";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Calibri", 10F);
            label12.Location = new Point(6, 43);
            label12.Name = "label12";
            label12.Size = new Size(42, 17);
            label12.TabIndex = 20;
            label12.Text = "Mask:";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new Font("Calibri", 10F);
            label13.Location = new Point(6, 25);
            label13.Name = "label13";
            label13.Size = new Size(23, 17);
            label13.TabIndex = 19;
            label13.Text = "IP:";
            // 
            // hostToolStripMenuItem
            // 
            hostToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { desbloquearToolStripMenuItem, bloqueioTotalToolStripMenuItem, bloqueioTotalExceptoOGmailToolStripMenuItem, bloqueioRedesSociaisToolStripMenuItem, bloqueioStreamToolStripMenuItem, bloqueioRedesSociaisEStreamToolStripMenuItem });
            hostToolStripMenuItem.Name = "hostToolStripMenuItem";
            hostToolStripMenuItem.Size = new Size(293, 22);
            hostToolStripMenuItem.Text = "Host";
            // 
            // desbloquearToolStripMenuItem
            // 
            desbloquearToolStripMenuItem.Name = "desbloquearToolStripMenuItem";
            desbloquearToolStripMenuItem.Size = new Size(254, 22);
            desbloquearToolStripMenuItem.Text = "Desbloquear";
            desbloquearToolStripMenuItem.Click += desbloquearToolStripMenuItem_Click;
            // 
            // bloqueioTotalToolStripMenuItem
            // 
            bloqueioTotalToolStripMenuItem.Name = "bloqueioTotalToolStripMenuItem";
            bloqueioTotalToolStripMenuItem.Size = new Size(254, 22);
            bloqueioTotalToolStripMenuItem.Text = "Bloqueio Total";
            bloqueioTotalToolStripMenuItem.Click += bloqueioTotalToolStripMenuItem_Click;
            // 
            // bloqueioTotalExceptoOGmailToolStripMenuItem
            // 
            bloqueioTotalExceptoOGmailToolStripMenuItem.Name = "bloqueioTotalExceptoOGmailToolStripMenuItem";
            bloqueioTotalExceptoOGmailToolStripMenuItem.Size = new Size(254, 22);
            bloqueioTotalExceptoOGmailToolStripMenuItem.Text = "Bloqueio Total Excepto o Gmail";
            bloqueioTotalExceptoOGmailToolStripMenuItem.Click += bloqueioTotalExceptoOGmailToolStripMenuItem_Click;
            // 
            // bloqueioRedesSociaisToolStripMenuItem
            // 
            bloqueioRedesSociaisToolStripMenuItem.Name = "bloqueioRedesSociaisToolStripMenuItem";
            bloqueioRedesSociaisToolStripMenuItem.Size = new Size(254, 22);
            bloqueioRedesSociaisToolStripMenuItem.Text = "Bloqueio Redes Sociais";
            bloqueioRedesSociaisToolStripMenuItem.Click += bloqueioRedesSociaisToolStripMenuItem_Click;
            // 
            // bloqueioStreamToolStripMenuItem
            // 
            bloqueioStreamToolStripMenuItem.Name = "bloqueioStreamToolStripMenuItem";
            bloqueioStreamToolStripMenuItem.Size = new Size(254, 22);
            bloqueioStreamToolStripMenuItem.Text = "Bloqueio Stream";
            bloqueioStreamToolStripMenuItem.Click += bloqueioStreamToolStripMenuItem_Click;
            // 
            // bloqueioRedesSociaisEStreamToolStripMenuItem
            // 
            bloqueioRedesSociaisEStreamToolStripMenuItem.Name = "bloqueioRedesSociaisEStreamToolStripMenuItem";
            bloqueioRedesSociaisEStreamToolStripMenuItem.Size = new Size(254, 22);
            bloqueioRedesSociaisEStreamToolStripMenuItem.Text = "Bloqueio Redes Sociais e Stream";
            bloqueioRedesSociaisEStreamToolStripMenuItem.Click += bloqueioRedesSociaisEStreamToolStripMenuItem_Click;
            // 
            // Dashboard
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.FundoClaro;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(800, 450);
            Controls.Add(InfoRede);
            Controls.Add(InfoDisco);
            Controls.Add(InfoMemoria);
            Controls.Add(InfoPC);
            Controls.Add(ckEstilo);
            Controls.Add(barraMenu);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = barraMenu;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Dashboard";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Dashboard";
            Load += Dashboard_Load;
            barraMenu.ResumeLayout(false);
            barraMenu.PerformLayout();
            InfoPC.ResumeLayout(false);
            InfoPC.PerformLayout();
            InfoMemoria.ResumeLayout(false);
            InfoMemoria.PerformLayout();
            InfoDisco.ResumeLayout(false);
            InfoDisco.PerformLayout();
            InfoRede.ResumeLayout(false);
            InfoRede.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip barraMenu;
        private ToolStripMenuItem dominioToolStripMenuItem;
        private ToolStripMenuItem testeDeDominioToolStripMenuItem;
        private ToolStripMenuItem sincronizaçãoDeTempoToolStripMenuItem;
        private ToolStripMenuItem windowsToolStripMenuItem;
        private ToolStripMenuItem testeDeDiscoEWindowsToolStripMenuItem;
        private ToolStripMenuItem correcãoDoErroRDCCredSSPToolStripMenuItem;
        private ToolStripMenuItem utilizadorToolStripMenuItem;
        private ToolStripMenuItem redeToolStripMenuItem;
        private ToolStripMenuItem actualizarComWingetToolStripMenuItem;
        private ToolStripMenuItem reiniciarServiçoDeImpressãoToolStripMenuItem;
        private ToolStripMenuItem testeDeComunicaçãoEAcessosÀRedeToolStripMenuItem;
        private ToolStripMenuItem correcçãoDoAdminToolStripMenuItem;
        private ToolStripMenuItem opçõesToolStripMenuItem;
        private ToolStripMenuItem actualizarToolStripMenuItem;
        private CheckBox ckEstilo;
        private GroupBox InfoPC;
        private Label txWINVersao;
        private Label txWINEdicao;
        private Label txProcessador;
        private Label txNomePC;
        private Label label4;
        private Label label3;
        private Label label2;
        private Label label1;
        private GroupBox InfoMemoria;
        private GroupBox InfoDisco;
        private GroupBox InfoRede;
        private Label lbMemLivre;
        private Label lbMemUso;
        private Label lbMemTotal;
        private Label label8;
        private Label label9;
        private Label label10;
        private Label lbDiskLivre;
        private Label lbDiskUso;
        private Label lbDiskTotal;
        private Label label14;
        private Label label15;
        private Label label16;
        private Label lbRedeGateway;
        private Label lbRedeMask;
        private Label lbRedeIP;
        private Label label11;
        private Label label12;
        private Label label13;
        private ToolStripMenuItem diagnósticoDeMemóriaToolStripMenuItem;
        private ToolStripMenuItem backupDeDriversToolStripMenuItem;
        private ToolStripMenuItem pontoDeRestauroToolStripMenuItem;
        private ToolStripMenuItem manutençãoDoWindowsToolStripMenuItem;
        private ToolStripMenuItem desinstalarComWingetToolStripMenuItem;
        private ToolStripMenuItem hostToolStripMenuItem;
        private ToolStripMenuItem desbloquearToolStripMenuItem;
        private ToolStripMenuItem bloqueioTotalToolStripMenuItem;
        private ToolStripMenuItem bloqueioTotalExceptoOGmailToolStripMenuItem;
        private ToolStripMenuItem bloqueioRedesSociaisToolStripMenuItem;
        private ToolStripMenuItem bloqueioStreamToolStripMenuItem;
        private ToolStripMenuItem bloqueioRedesSociaisEStreamToolStripMenuItem;
    }
}