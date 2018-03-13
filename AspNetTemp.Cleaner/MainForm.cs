using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using AndreiSmallUtils.Utils;

namespace AspNetTemp.Cleaner
{
    public partial class MainForm : Form
    {
        #region Constants

        private const int LOADER_COUNTER_MAX = 30; //check every 30 seconds if there is new files
        private const string TEMP_FILES_PATH = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Temporary ASP.NET Files";
        private const int FORCE_COUNT_TIME = 3;

        #endregion

        #region Fields

        private int _loaderCounter;

        #endregion

        #region Properties

        public bool Loading
        {
            get
            {
                return !Enabled;
            }
            set
            {
                if(Loading == value)
                    return;

                lblLoading.Visible = value;
                Enabled = !value;
            }
        }

        #endregion

        #region Constructors

        public MainForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Event Handlers

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadTempFolders();
        }

        private void tsmiSelectAll_Click(object sender, EventArgs e)
        {
            SetListViewChecked(true);
        }

        private void tsmiDeselectAll_Click(object sender, EventArgs e)
        {
            SetListViewChecked(false);
        }

        private void tsmiRefresh_Click(object sender, EventArgs e)
        {
            LoadTempFolders();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                Loading = true;

                var remainedFolders = new StringBuilder();
                foreach (ListViewItem item in lvFiles.Items)
                {
                    if(!item.Checked)
                        continue;
                
                    var count = 1;
                    bool deleted;

                    if (chkForce.Checked)
                        count = FORCE_COUNT_TIME;

                    do
                    {
                        var path = item.Tag.ToString();
                        try
                        {
                            Directory.Delete(path, true);
                            deleted = !Directory.Exists(path);
                        }
                        catch (Exception mex)
                        {
                            Console.WriteLine(mex);
                            deleted = false;
                        }
                    } while (count > 0 && !deleted);

                    if (!deleted)
                    {
                        remainedFolders.Append("\n");
                        remainedFolders.Append(item.SubItems[0]);
                    }
                }

                if (remainedFolders.Length > 0)
                    MessageBox.Show(this,
                                    "Some file cannot be deleted: " + remainedFolders,
                                    Text,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

                LoadTempFolders();
            }
            finally
            {
                Loading = false;
            }
        }
        
        private void tmrLoader_Tick(object sender, EventArgs e)
        {
            if (lvFiles.Items.Count > 0)
            {
                tmrLoader.Stop();
                return;
            }

            if (_loaderCounter >= LOADER_COUNTER_MAX)
            {
                tmrLoader.Stop();
                _loaderCounter = 0;
                LoadTempFolders();
            }
            else
                _loaderCounter++;
        }

        private void lvFiles_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            btnDelete.Enabled = lvFiles.Items.OfType<ListViewItem>().Any(lvi => lvi.Checked);
        }

        #endregion

        #region Private methods

        private void SetListViewChecked(bool check)
        {
            foreach (ListViewItem item in lvFiles.Items)
            {
                item.Checked = check;
            }
        }

        private void LoadTempFolders()
        {
            try
            {
                Loading = true;

                lvFiles.BeginUpdate();

                lvFiles.Items.Clear();

                var items = Directory.GetDirectories(TEMP_FILES_PATH);

                if(items.Length <= 0)
                    return;

                lvFiles.Items.AddRange(items.Select(CreateListViewItem).ToArray());

                SetListViewChecked(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                lvFiles.EndUpdate();
                btnDelete.Enabled = lvFiles.Items.OfType<ListViewItem>().Any(lvi => lvi.Checked);
                Loading = false;

                if (lvFiles.Items.Count <= 0)
                    tmrLoader.Start();
            }
        }

        private static ListViewItem CreateListViewItem(string path)
        {
            var di = new DirectoryInfo(path);
            return new ListViewItem(di.Name)
            {
                SubItems =
                {
                    SizeUtils.DirectorySize(di).ToFileSize()
                },
                Tag = path
            };
        }

        #endregion
    }
}
