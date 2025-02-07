using System;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Windows.Forms;
using Etrea3.Core;

namespace Etrea_Admin
{
    internal static class APIHelper
    {
        private static readonly string APIKey = ConfigurationManager.AppSettings["APIKey"];
        private static readonly string APIUrl = ConfigurationManager.AppSettings["APIUrl"];

        internal static async Task<T> LoadAssets<T>(string apiPath, bool hideErrors) where T : class
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-API-Key", APIKey);
                var response = await client.GetAsync($"{APIUrl}{apiPath}");
                if (response == null || response.StatusCode != HttpStatusCode.OK && !hideErrors)
                {
                    MessageBox.Show($"API response was null or not OK: {response.StatusCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return null;
                }
                var json = await response.Content.ReadAsStringAsync();
                try
                {
                    var ds = JsonConvert.DeserializeObject<string>(json);
                    var etreaObject = Helpers.DeserialiseEtreaObject<T>(ds);
                    return etreaObject;
                }
                catch (Exception ex)
                {
                    if (!hideErrors)
                    {
                        MessageBox.Show($"Error processing API response: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return null;
                }
            }
        }

        internal static async Task<bool> AddNewAsset(string apiPath, string objectJson)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", APIKey);
                    var content = new StringContent(objectJson, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{APIUrl}{apiPath}", content);
                    var apiResponse = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show(apiResponse, "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show($"Failed to create Asset: {apiResponse}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding new Asset: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        internal static async Task<bool> UpdateExistingAsset(string apiPath, string objectJson)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", APIKey);
                    var content = new StringContent(objectJson, Encoding.UTF8, "application/json");
                    var response = await client.PutAsync($"{APIUrl}{apiPath}", content);
                    var apiResponse = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show(apiResponse, "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show($"Failed to update Asset: {apiResponse}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating Asset: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        internal static async Task<bool> DeleteExistingAsset(string apiPath)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", APIKey);
                    var response = await client.DeleteAsync($"{APIUrl}{apiPath}");
                    var apiResponse = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Asset deleted successfully.", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show($"Failed to delete Asset: {apiResponse}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting Asset: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }
    }
}
