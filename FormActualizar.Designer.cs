namespace Ferramenta_IT
{
    partial class FormActualizar
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormActualizar));
            checkedListBoxApps = new CheckedListBox();
            labelStatus = new Label();
            buttonCancel = new Button();
            buttonUpdate = new Button();
            SuspendLayout();
            // 
            // checkedListBoxApps
            // 
            checkedListBoxApps.FormattingEnabled = true;
            checkedListBoxApps.Location = new Point(12, 27);
            checkedListBoxApps.Name = "checkedListBoxApps";
            checkedListBoxApps.Size = new Size(760, 274);
            checkedListBoxApps.TabIndex = 0;
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.Location = new Point(12, 9);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(139, 15);
            labelStatus.TabIndex = 1;
            labelStatus.Text = "A procurar atualizações...";
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(697, 307);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 30);
            buttonCancel.TabIndex = 2;
            buttonCancel.Text = "Cancelar";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // buttonUpdate
            // 
            buttonUpdate.Location = new Point(616, 307);
            buttonUpdate.Name = "buttonUpdate";
            buttonUpdate.Size = new Size(75, 30);
            buttonUpdate.TabIndex = 3;
            buttonUpdate.Text = "Actualizar";
            buttonUpdate.UseVisualStyleBackColor = true;
            buttonUpdate.Click += button2_Click;
            // 
            // FormActualizar
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 349);
            Controls.Add(buttonUpdate);
            Controls.Add(buttonCancel);
            Controls.Add(labelStatus);
            Controls.Add(checkedListBoxApps);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormActualizar";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Atualizar Aplicativos com Winget";
            Load += FormActualizar_LoadAsync;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckedListBox checkedListBoxApps;
        private Label labelStatus;
        private Button buttonCancel;
        private Button buttonUpdate;
    }
}