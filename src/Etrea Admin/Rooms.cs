using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Etrea3.Objects;
using System.Linq;
using Newtonsoft.Json;
using Etrea3;

namespace Etrea_Admin
{
    public partial class Form1 : Form
    {
        private static List<Room> allRooms = new List<Room>();
        private static ListViewItemComparer roomListViewComparer;

        #region Event Handlers
        private async void btnRoomAddTickItem_Click(object sender, EventArgs e)
        {
            using (var kVal = new GetIntegerPair("Enter Item Details", "Item ID:", "Amount:"))
            {
                if (kVal.ShowDialog() == DialogResult.OK)
                {
                    int id = kVal.id;
                    int amount = kVal.amount;
                    var npc = await APIHelper.LoadAssets<InventoryItem>($"/item/{id}", false);
                    if (npc == null)
                    {
                        MessageBox.Show($"The provided ID is not valid for any current Item template.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    var item = listViewRoomTickItems.Items.Cast<ListViewItem>().FirstOrDefault(x => Convert.ToInt32(x.SubItems[0].Text) == id);
                    int baseAmount = item != null ? Convert.ToInt32(item.SubItems[2].Text) : 0;
                    baseAmount += amount;
                    if (item != null)
                    {
                        item.SubItems[2].Text = baseAmount.ToString();
                    }
                    else
                    {
                        listViewRoomTickItems.Items.Add(new ListViewItem(new[]
                        {
                            id.ToString(),
                            npc.Name,
                            amount.ToString(),
                        }));
                    }
                }
            }
        }

        private void btnRoomRemoveTickItem_Click(object sender, EventArgs e)
        {
            if (listViewRoomTickItems.SelectedItems.Count > 0)
            {
                var obj = listViewRoomTickItems.SelectedItems[0];
                listViewRoomTickItems.Items.Remove(obj);
            }
        }

        private void btnRoomClearTickItems_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the list of Tick Items for this Room?", "Confirm?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listViewRoomTickItems.Items.Clear();
            }
        }

        private async void btnRoomAddStartItem_Click(object sender, EventArgs e)
        {
            using (var kVal = new GetIntegerPair("Enter Item Details", "Item ID:", "Amount:"))
            {
                if (kVal.ShowDialog() == DialogResult.OK)
                {
                    int id = kVal.id;
                    int amount = kVal.amount;
                    var npc = await APIHelper.LoadAssets<InventoryItem>($"/item/{id}", false);
                    if (npc == null)
                    {
                        MessageBox.Show($"The provided ID is not valid for any current Item template.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    var item = listViewRoomStartItems.Items.Cast<ListViewItem>().FirstOrDefault(x => Convert.ToInt32(x.SubItems[0].Text) == id);
                    int baseAmount = item != null ? Convert.ToInt32(item.SubItems[2].Text) : 0;
                    baseAmount += amount;
                    if (item != null)
                    {
                        item.SubItems[2].Text = baseAmount.ToString();
                    }
                    else
                    {
                        listViewRoomStartItems.Items.Add(new ListViewItem(new[]
                        {
                            id.ToString(),
                            npc.Name,
                            amount.ToString(),
                        }));
                    }
                }
            }
        }

        private void btnRoomRemoveStartItem_Click(object sender, EventArgs e)
        {
            if (listViewRoomStartItems.SelectedItems.Count > 0)
            {
                var obj = listViewRoomStartItems.SelectedItems[0];
                listViewRoomStartItems.Items.Remove(obj);
            }
        }

        private void btnClearRoomStartItems_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the list of Startup Items for this Room?", "Confirm?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listViewRoomStartItems.Items.Clear();
            }
        }

        private async void btnRoomAddTickNPC_Click(object sender, EventArgs e)
        {
            using (var kVal = new GetIntegerPair("Enter NPC Details", "NPC ID:", "Amount:"))
            {
                if (kVal.ShowDialog() == DialogResult.OK)
                {
                    int id = kVal.id;
                    int amount = kVal.amount;
                    var npc = await APIHelper.LoadAssets<NPC>($"/npc/{id}", false);
                    if (npc == null)
                    {
                        MessageBox.Show($"The provided ID is not valid for any current NPC template.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    var item = listViewRoomTickNPCs.Items.Cast<ListViewItem>().FirstOrDefault(x => Convert.ToInt32(x.SubItems[0].Text) == id);
                    int baseAmount = item != null ? Convert.ToInt32(item.SubItems[2].Text) : 0;
                    baseAmount += amount;
                    if (item != null)
                    {
                        item.SubItems[2].Text = baseAmount.ToString();
                    }
                    else
                    {
                        listViewRoomTickNPCs.Items.Add(new ListViewItem(new[]
                        {
                            id.ToString(),
                            npc.Name,
                            amount.ToString(),
                        }));
                    }
                }
            }
        }

        private void btnRoomRemoveTickNPC_Click(object sender, EventArgs e)
        {
            if (listViewRoomTickNPCs.SelectedItems.Count > 0)
            {
                var obj = listViewRoomTickNPCs.SelectedItems[0];
                listViewRoomTickNPCs.Items.Remove(obj);
            }
        }

        private void btnRoomClearTickNPCs_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the list of Tick Spawn NPCs for this Room?", "Confirm?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listViewRoomTickNPCs.Items.Clear();
            }
        }

        private async void btnAddRoomStartNPC_Click(object sender, EventArgs e)
        {
            using (var kVal = new GetIntegerPair("Enter NPC Details", "NPC ID:", "Amount:"))
            {
                if (kVal.ShowDialog() == DialogResult.OK)
                {
                    int id = kVal.id;
                    int amount = kVal.amount;
                    var npc = await APIHelper.LoadAssets<NPC>($"/npc/{id}", false);
                    if (npc == null)
                    {
                        MessageBox.Show($"The provided ID is not valid for any current NPC template.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    var item = listViewRoomStartNPCs.Items.Cast<ListViewItem>().FirstOrDefault(x => Convert.ToInt32(x.SubItems[0].Text) == id);
                    int baseAmount = item != null ? Convert.ToInt32(item.SubItems[2].Text) : 0;
                    baseAmount += amount;
                    if (item != null)
                    {
                        item.SubItems[2].Text = baseAmount.ToString();
                    }
                    else
                    {
                        listViewRoomStartNPCs.Items.Add(new ListViewItem(new[]
                        {
                            id.ToString(),
                            npc.Name,
                            amount.ToString(),
                        }));
                    }
                }
            }
        }

        private void btnRemoveRoomStartNPC_Click(object sender, EventArgs e)
        {
            if (listViewRoomStartNPCs.SelectedItems.Count > 0)
            {
                var obj = listViewRoomStartNPCs.SelectedItems[0];
                listViewRoomStartNPCs.Items.Remove(obj);
            }
        }

        private void btnClearRoomStartNPCs_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the list of Startup NPCs for this Room?", "Confirm?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listViewRoomStartNPCs.Items.Clear();
            }
        }

        private void btnLoadRooms_Click(object sender, EventArgs e)
        {
            GetRooms();
        }

        private async void btnAddRoom_Click(object sender, EventArgs e)
        {
            if (!ValidateRoom())
            {
                return;
            }
            Room r = GetRoomFromFormData();
            if (r == null)
            {
                return;
            }
            btnAddRoom.Enabled = false;
            var roomJson = JsonConvert.SerializeObject(r);
            if (await APIHelper.AddNewAsset("/room", roomJson))
            {
                GetRooms();
                ClearRoomForm();
            }
            btnAddRoom.Enabled = true;
        }

        private async void btnUpdateRoom_Click(object sender, EventArgs e)
        {
            if (!ValidateRoom())
            {
                return;
            }
            Room r = GetRoomFromFormData();
            if (r == null)
            {
                return;
            }
            btnUpdateRoom.Enabled = false;
            var roomJson = JsonConvert.SerializeObject(r);
            if (await APIHelper.UpdateExistingAsset("/room", roomJson))
            {
                GetRooms();
                ClearRoomForm();
            }
            btnUpdateRoom.Enabled = true;
        }

        private void btnClearRoom_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the form fields?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ClearRoomForm();
            }
        }

        private async void btnDeleteRoom_Click(object sender, EventArgs e)
        {
            if (listViewRooms.SelectedItems.Count == 0)
            {
                return;
            }
            btnDeleteRoom.Enabled = false;
            var rm = allRooms.FirstOrDefault(x => x.ID == Convert.ToInt32(listViewRooms.SelectedItems[0].SubItems[0].Text));
            if (rm != null && MessageBox.Show("Delete the selected Room? This action cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (await APIHelper.DeleteExistingAsset($"/room/{rm.ID}"))
                {
                    GetRooms();
                    ClearRoomForm();
                }
            }
            btnDeleteRoom.Enabled = true;
        }

        private void rTxtRoomLongDesc_KeyUp(object sender, KeyEventArgs e)
        {
            int currentLineIndex = rTxtRoomLongDesc.GetLineFromCharIndex(rTxtRoomLongDesc.SelectionStart);
            string currentLine = rTxtRoomLongDesc.Lines.Length > currentLineIndex ? rTxtRoomLongDesc.Lines[currentLineIndex] : string.Empty;
            lblRoomLongDescLength.Text = $"Line Length: {currentLine.Length} characters";
        }

        private void rTxtRoomSign_KeyUp(object sender, KeyEventArgs e)
        {
            int currentLineIndex = rTxtRoomSign.GetLineFromCharIndex(rTxtRoomSign.SelectionStart);
            string currentLine = rTxtRoomSign.Lines.Length > currentLineIndex ? rTxtRoomSign.Lines[currentLineIndex] : string.Empty;
            lblRoomSignLength.Text = $"Line Length: {currentLine.Length} characters";
        }

        private async void listViewRooms_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearRoomForm();
            if (listViewRooms.SelectedItems.Count > 0)
            {
                var room = allRooms.FirstOrDefault(x => x.ID == Convert.ToInt32(listViewRooms.SelectedItems[0].SubItems[0].Text));
                if (room == null)
                {
                    return;
                }
                txtBxRoomRoomID.Text = room.ID.ToString();
                txtBxRoomZoneID.Text = room.ZoneID.ToString();
                txtBxRoomName.Text = room.RoomName;
                txtBxRoomFlags.Text = room.Flags.ToString();
                txtBxRoomShortDesc.Text = room.ShortDescription;
                rTxtRoomLongDesc.Text = room.LongDescription;
                rTxtRoomSign.Text = room.SignText;
                if (room.RoomExits.Count > 0)
                {
                    foreach (var exit in room.RoomExits.Values)
                    {
                        listViewRoomExits.Items.Add(new ListViewItem(new[]
                        {
                            exit.ExitDirection,
                            exit.DestinationRoomID.ToString(),
                            exit.RequiredSkill.ToString(),
                        }));
                    }
                }
                if (room.StartingNPCs.Count > 0)
                {
                    foreach(var n in room.StartingNPCs)
                    {
                        var npc = await APIHelper.LoadAssets<NPC>($"/npc/{n.Key}", true);
                        string npcName = npc != null ? npc.Name : "Invalid NPC ID";
                        listViewRoomStartNPCs.Items.Add(new ListViewItem(new[]
                        {
                            n.Key.ToString(),
                            npcName,
                            n.Value.ToString()
                        }));
                    }
                }
                if (room.StartingItems.Count > 0)
                {
                    foreach(var i in room.StartingItems)
                    {
                        var item = await APIHelper.LoadAssets<InventoryItem>($"/item/{i.Key}", true);
                        string itemName = item != null ? item.Name : "Invalid Item ID";
                        listViewRoomStartItems.Items.Add(new ListViewItem(new[]
                        {
                            i.Key.ToString(),
                            itemName,
                            i.Value.ToString()
                        }));
                    }
                }
                if (room.SpawnNPCsOnTick.Count > 0)
                {
                    foreach(var n in room.SpawnNPCsOnTick)
                    {
                        var npc = await APIHelper.LoadAssets<NPC>($"/npc/{n.Key}", true);
                        string npcName = npc != null ? npc.Name : "Invalid NPC ID";
                        listViewRoomTickNPCs.Items.Add(new ListViewItem(new[]
                        {
                            n.Key.ToString(),
                            npcName,
                            n.Value.ToString()
                        }));
                    }
                }
                if (room.SpawnItemsOnTick.Count > 0)
                {
                    foreach (var i in room.SpawnItemsOnTick)
                    {
                        var item = await APIHelper.LoadAssets<InventoryItem>($"/item/{i.Key}", true);
                        var itemName = item != null ? item.Name : "Invalid Item ID";
                        listViewRoomTickItems.Items.Add(new ListViewItem(new[]
                        {
                            i.Key.ToString(),
                            itemName,
                            i.Value.ToString()
                        }));
                    }
                }
            }
        }

        private void btnAddRoomExit_Click(object sender, EventArgs e)
        {
            using (var newExit = new AddRoomExit())
            {
                if (newExit.ShowDialog() == DialogResult.OK)
                {
                    listViewRoomExits.Items.Add(new ListViewItem(new[]
                    {
                        newExit.roomExit.ExitDirection,
                        newExit.roomExit.DestinationRoomID.ToString(),
                        newExit.roomExit.RequiredSkill
                    }));
                }
            }
        }

        private void btnRemoveRoomExit_Click(object sender, EventArgs e)
        {
            if (listViewRoomExits.SelectedItems.Count > 0)
            {
                var obj = listViewRoomExits.SelectedItems[0];
                listViewRoomExits.Items.Remove(obj);
            }
        }

        private void btnClearRoomExits_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear all Exits?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listViewRoomExits.Items.Clear();
            }
        }

        private void listViewRooms_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (roomListViewComparer == null)
            {
                roomListViewComparer = new ListViewItemComparer(e.Column, SortOrder.Ascending);
            }
            if (e.Column == roomListViewComparer.SortColumn)
            {
                roomListViewComparer.SortOrder = roomListViewComparer.SortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                roomListViewComparer.SortOrder = SortOrder.Ascending;
                roomListViewComparer.SortColumn = e.Column;
            }
            listViewRooms.ListViewItemSorter = roomListViewComparer;
            listViewRooms.Sort();
        }
        #endregion

        #region Functions
        private void ClearRoomForm()
        {
            txtBxRoomRoomID.Clear();
            txtBxRoomZoneID.Clear();
            txtBxRoomName.Clear();
            txtBxRoomShortDesc.Clear();
            txtBxRoomFlags.Clear();
            rTxtRoomLongDesc.Clear();
            rTxtRoomSign.Clear();
            chkBoxRoomLongDescOverride.Checked = false;
            chkBoxRoomSignOverride.Checked = false;
            listViewRoomExits.Items.Clear();
            listViewRoomStartNPCs.Items.Clear();
            listViewRoomStartItems.Items.Clear();
            listViewRoomTickNPCs.Items.Clear();
            listViewRoomTickItems.Items.Clear();
            lblRoomLongDescLength.Text = "Line Length: 0 characters";
            lblRoomSignLength.Text = "Line Length: 0 characters";
        }

        private async void GetRooms()
        {
            btnLoadRooms.Enabled = false;
            ClearRoomForm();
            var result = await APIHelper.LoadAssets<List<Room>>("/room", false);
            if (result != null)
            {
                allRooms.Clear();
                listViewRooms.Items.Clear();
                foreach (var room in result.OrderBy(x => x.ID))
                {
                    allRooms.Add(room);
                    listViewRooms.Items.Add(new ListViewItem(new[]
                    {
                        room.ID.ToString(),
                        room.ZoneID.ToString(),
                        room.RoomName,
                        room.ShortDescription,
                        room.RoomExits.Count().ToString(),
                        room.Flags.ToString(),
                        room.StartingNPCs.Count().ToString(),
                        room.StartingItems.Count().ToString(),
                        room.SpawnNPCsOnTick.Count().ToString(),
                        room.SpawnItemsOnTick.Count().ToString(),
                    }));
                }
                foreach (ColumnHeader h in listViewRooms.Columns)
                {
                    h.Width = -2;
                }
            }
            btnLoadRooms.Enabled = true;
        }

        private Room GetRoomFromFormData()
        {
            Room room = new Room();
            try
            {
                room.ID = Convert.ToInt32(txtBxRoomRoomID.Text);
                room.RoomName = txtBxRoomName.Text;
                room.ShortDescription = txtBxRoomShortDesc.Text;
                room.ZoneID = Convert.ToInt32(txtBxRoomZoneID.Text);
                room.LongDescription = rTxtRoomLongDesc.Lines.ConvertToString();
                room.SignText = rTxtRoomSign.Lines.ConvertToString();
                Enum.TryParse(txtBxRoomFlags.Text, true, out RoomFlags result);
                room.Flags = result;
                if (listViewRoomExits.Items.Count > 0)
                {
                    foreach (ListViewItem item in listViewRoomExits.Items)
                    {
                        var exit = new RoomExit
                        {
                            ExitDirection = item.SubItems[0].Text,
                            DestinationRoomID = Convert.ToInt32(item.SubItems[1].Text),
                            RequiredSkill = item.SubItems[2].Text,
                        };
                        room.RoomExits.TryAdd(item.SubItems[0].Text, exit);
                    }
                }
                if (listViewRoomStartNPCs.Items.Count > 0)
                {
                    foreach (ListViewItem item in listViewRoomStartNPCs.Items)
                    {
                        var npcID = Convert.ToInt32(item.SubItems[0].Text);
                        var amount = Convert.ToInt32(item.SubItems[2].Text);
                        room.StartingNPCs.TryAdd(npcID, amount);
                    }
                }
                if (listViewRoomStartItems.Items.Count > 0)
                {
                    foreach (ListViewItem item in listViewRoomStartItems.Items)
                    {
                        var itemID = Convert.ToInt32(item.SubItems[0].Text);
                        var amount = Convert.ToInt32(item.SubItems[2].Text);
                        room.StartingItems.TryAdd(itemID, amount);
                    }
                }
                if (listViewRoomTickNPCs.Items.Count > 0)
                {
                    foreach (ListViewItem item in listViewRoomTickNPCs.Items)
                    {
                        var npcID = Convert.ToInt32(item.SubItems[0].Text);
                        var amount = Convert.ToInt32(item.SubItems[2].Text);
                        room.SpawnNPCsOnTick.TryAdd(npcID, amount);
                    }
                }
                if (listViewRoomTickItems.Items.Count > 0)
                {
                    foreach (ListViewItem item in listViewRoomTickItems.Items)
                    {
                        var itemID = Convert.ToInt32(item.SubItems[0].Text);
                        var amount = Convert.ToInt32(item.SubItems[2].Text);
                        room.SpawnItemsOnTick.TryAdd(itemID, amount);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating Room object: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                room = null;
            }
            return room;
        }

        private bool ValidateRoom()
        {
            if (string.IsNullOrEmpty(txtBxRoomRoomID.Text))
            {
                MessageBox.Show("You must provide a Room ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxRoomName.Text))
            {
                MessageBox.Show("You must provide a Room Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxRoomShortDesc.Text))
            {
                MessageBox.Show("You must provide a Short Description.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(txtBxRoomZoneID.Text))
            {
                MessageBox.Show("You must provide a Zone ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(rTxtRoomLongDesc.Text))
            {
                MessageBox.Show("You must provide a Long Description.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (txtBxRoomFlags.Text.IndexOf("Sign", StringComparison.OrdinalIgnoreCase) >=0 && string.IsNullOrEmpty(rTxtRoomSign.Text))
            {
                MessageBox.Show("If the Room has a Sign, you must provide Sign text.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!chkBoxRoomLongDescOverride.Checked)
            {
                foreach (var line in rTxtRoomLongDesc.Lines)
                {
                    if (line.Length > 80)
                    {
                        MessageBox.Show("One or more lines in Long Description are longer than 80 characters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            if (!chkBoxRoomSignOverride.Checked)
            {
                foreach (var line in rTxtRoomSign.Lines)
                {
                    if (line.Length > 80)
                    {
                        MessageBox.Show("One or more lines in Sign Text are longer than 80 characters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            return true;
        }
        #endregion
    }
}