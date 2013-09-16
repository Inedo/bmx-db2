using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.DB2
{
    /// <summary>
    /// Custom editor for the DB2 database provider.
    /// </summary>
    internal sealed class DB2DatabaseProviderEditor : ProviderEditorBase
    {
        private ValidatingTextBox txtConnectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="DB2DatabaseProviderEditor"/> class.
        /// </summary>
        public DB2DatabaseProviderEditor()
        {
        }

        public override void BindToForm(ProviderBase extension)
        {
            EnsureChildControls();

            var db2 = (DB2DatabaseProvider)extension;
            txtConnectionString.Text = db2.ConnectionString;
        }
        public override ProviderBase CreateFromForm()
        {
            EnsureChildControls();

            return new DB2DatabaseProvider
            {
                ConnectionString = txtConnectionString.Text
            };
        }

        protected override void CreateChildControls()
        {
            this.txtConnectionString = new ValidatingTextBox
            {
                Width = 300,
                Required = true
            };

            CUtil.Add(this,
                new FormFieldGroup(
                    "Connection String",
                    "The connection string to the DB2 database. The standard format for this is:<br /><br />"
                    + "<em>Server=myServerAddress; Database=myDataBase; UID=myUsername; PWD=myPassword;</em>",
                    false,
                    new StandardFormField("Connection String:", this.txtConnectionString)
                )
            );

            base.CreateChildControls();
        }
    }
}
