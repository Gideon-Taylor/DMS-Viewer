using DMSLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DMS_Viewer
{
    public partial class RecordMetadataViewer : Form
    {
        private DMSRecordMetadata metadata;
        public RecordMetadataViewer(DMSRecordMetadata metadata)
        {
            InitializeComponent();
            this.metadata = metadata;
            txtBuildSeq.Text = metadata.BuildSequence.ToString();
            txtDeleteRecord.Text = metadata.AnalyticDeleteRecord;
            txtFieldCount.Text = metadata.FieldCount.ToString();
            txtIndexCount.Text = metadata.IndexCount.ToString();
            txtOptTriggers.Text = metadata.OptimizationTriggers;
            txtOwner.Text = metadata.OwnerID;
            txtParentRecord.Text = metadata.ParentRecord;
            txtRecordDBName.Text = metadata.RecordDBName;
            txtRecordLang.Text = metadata.RecordLanguage;
            txtRecordName.Text = metadata.RecordName;
            txtRelLang.Text = metadata.RelatedLanguageRecord;
            txtVersion2.Text = metadata.VersionNumber2.ToString();
            txtVersionNum.Text = metadata.VersionNumber.ToString();
            if (metadata.Tablespaces.Count == 1)
            {
                txtTablespace.Text = metadata.Tablespaces[0].TablespaceName;
            } else
            {
                txtTablespace.Enabled = false;
            }

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            /* Update the metdata fields with the new values */
            metadata.BuildSequence = int.Parse(txtBuildSeq.Text);
            metadata.AnalyticDeleteRecord = txtDeleteRecord.Text;
            metadata.FieldCount = int.Parse(txtFieldCount.Text);
            metadata.IndexCount = int.Parse(txtIndexCount.Text);
            metadata.OptimizationTriggers = txtOptTriggers.Text;
            metadata.OwnerID = txtOwner.Text;
            metadata.ParentRecord = txtParentRecord.Text;
            metadata.RecordDBName = txtRecordDBName.Text;
            metadata.RecordLanguage = txtRecordLang.Text;
            metadata.RecordName = txtRecordName.Text;
            metadata.RelatedLanguageRecord = txtRelLang.Text;
            metadata.VersionNumber = int.Parse(txtVersionNum.Text);
            metadata.VersionNumber2 = int.Parse(txtVersion2.Text);

            if (metadata.Tablespaces.Count == 1)
            {
                metadata.Tablespaces[0].TablespaceName = txtTablespace.Text;
            }

        }
    }
}
