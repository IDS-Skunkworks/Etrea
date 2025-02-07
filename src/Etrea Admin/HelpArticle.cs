using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Etrea3.Objects;
using System.Linq;
using Etrea3.Core;

namespace Etrea_Admin
{
    public partial class Form1 : Form
    {
        private List<HelpArticle> allArticles = new List<HelpArticle>();

        #region EventHandlers
        private void rtxtBxArticleText_KeyUp(object sender, KeyEventArgs e)
        {
            int currentLineIndex = rtxtBxArticleText.GetLineFromCharIndex(rtxtBxArticleText.SelectionStart);
            string currentLine = rtxtBxArticleText.Lines.Length > currentLineIndex ? rtxtBxArticleText.Lines[currentLineIndex] : string.Empty;
            lblArticleLineLength.Text = $"Line Length: {currentLine.Length} characters";
        }

        private void listBoxHelpArticles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxHelpArticles.SelectedItems.Count == 0)
            {
                return;
            }
            var article = allArticles.FirstOrDefault(x => x.Title == listBoxHelpArticles.SelectedItem.ToString());
            if (article == null)
            {
                return;
            }
            txtBxArticleName.Text = article.Title;
            chkBxArticleImmOnly.Checked = article.ImmOnly;
            rtxtBxArticleText.Text = article.ArticleText;
        }

        private async void btnAddArticle_Click(object sender, EventArgs e)
        {
            if (!ValidateArticle())
            {
                return;
            }
            var newArticle = GetArticleFromData();
            if (newArticle == null)
            {
                return;
            }
            var articleJson = Helpers.SerialiseEtreaObject<HelpArticle>(newArticle);
            btnAddArticle.Enabled = false;
            if (await APIHelper.AddNewAsset("/help", articleJson))
            {
                LoadArticles();
            }
            btnAddArticle.Enabled = true;
        }

        private async void btnUpdateArticle_Click(object sender, EventArgs e)
        {
            if (!ValidateArticle())
            {
                return;
            }
            var newArticle = GetArticleFromData();
            if (newArticle == null)
            {
                return;
            }
            var articleJson = Helpers.SerialiseEtreaObject<HelpArticle>(newArticle);
            btnUpdateArticle.Enabled = false;
            if (await APIHelper.UpdateExistingAsset("/help", articleJson))
            {
                LoadArticles();
            }
            btnUpdateArticle.Enabled = true;
        }

        private void btnLoadArticles_Click(object sender, EventArgs e)
        {
            LoadArticles();
        }

        private void btnClearArticleForm_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the Form?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ClearArticleForm();
            }
        }

        private async void btnDeleteArtricle_Click(object sender, EventArgs e)
        {
            if (listBoxHelpArticles.SelectedItems.Count == 0)
            {
                return;
            }
            var article = allArticles.FirstOrDefault(x => x.Title == listBoxHelpArticles.SelectedItem.ToString());
            if (article == null)
            {
                return;
            }
            if (MessageBox.Show("Delete the selected Article? This cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                btnDeleteArtricle.Enabled = false;
                if (await APIHelper.DeleteExistingAsset($"/help/name/{article.Title}"))
                {
                    LoadArticles();
                }
                btnDeleteArtricle.Enabled = true;
            }
        }
        #endregion

        #region Functions
        private async void LoadArticles()
        {
            allArticles.Clear();
            listBoxHelpArticles.Items.Clear();
            ClearArticleForm();
            var result = await APIHelper.LoadAssets<List<HelpArticle>>("/help", false);
            if (result != null)
            {
                foreach (var item in result.OrderBy(x => x.Title))
                {
                    allArticles.Add(item);
                    listBoxHelpArticles.Items.Add(item.Title);
                }
            }
        }

        private bool ValidateArticle()
        {
            if (string.IsNullOrEmpty(txtBxArticleName.Text))
            {
                MessageBox.Show("The Article must have a Name/Title", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(rtxtBxArticleText.Text))
            {
                MessageBox.Show("The Article must have a body.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!chkBxArticleLineLength.Checked)
            {
                foreach (var ln in rtxtBxArticleText.Lines)
                {
                    if (ln.Length > 80)
                    {
                        MessageBox.Show("One or more lines in the Article body are longer than 80 characters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            return true;
        }

        private HelpArticle GetArticleFromData()
        {
            HelpArticle article = new HelpArticle();
            try
            {
                article.Title = txtBxArticleName.Text;
                article.ArticleText = rtxtBxArticleText.Text;
                article.ImmOnly = chkBxArticleImmOnly.Checked;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating Article object: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                article = null;
            }
            return article;
        }

        private void ClearArticleForm()
        {
            rtxtBxArticleText.Clear();
            txtBxArticleName.Clear();
            chkBxArticleImmOnly.Checked = false;
        }
        #endregion
    }
}