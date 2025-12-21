using System;
using System.Collections.ObjectModel;
using System.ComponentModel;


//using System.Collect.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using static ShortCutKeysLib.KeysConfWindow;
using static ShortCutKeysLib.KeySequenceMngr;

namespace ShortCutKeysLib
{
    public class KeysConfWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ShortCutListViewModel> ShortcutVMs { get; }

        private ShortCutListViewModel selectedShortcut;
        public ShortCutListViewModel SelectedShortcut
        {
            get => selectedShortcut;
            set
            {
                if (selectedShortcut != value)
                {
                    //var kss = selectedShortcut.KeySeqStr;
                    selectedShortcut = value;
                    OnPropertyChanged();
                }
            }
        }

        public KeysConfWindowViewModel()
        {
            ShortcutVMs = new ObservableCollection<ShortCutListViewModel>()
            {
                //new ShortCutListViewModel
                //{
                //    KeyCombSequence = new KeyCombSequence("Test", "Desc",
                //        new List<KeyComb>(){ new KeyComb() { 
                //            Key = Key.A, Mods = ModifierKeys.Control 
                //        } }, (obj) => System.Diagnostics.Debug.WriteLine("aa"), null)
                //},
            };
        }
        
        private string? notifStr;
        public string? NotifStr
        {
            get
            {
                return notifStr;
            }
            set
            {
                notifStr = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ShortCutListViewModel : INotifyPropertyChanged
    {
        public string?          Name        { get { return KeyCombSequence.Name; }}
        private string? keySeqStr;
        public string?          KeySeqStr   { get { 
                return keySeqStr;
            }
            set { 
                keySeqStr = value;
                OnPropertyChanged();
            }
        }

        public string?          Description { get { return KeyCombSequence.Desc; } }
        public List<KeyComb>? KeyCombs { get { return KeyCombSequence?.KeyCombs; } 
            set {
                if (KeyCombSequence != null) {  
                    KeyCombSequence.KeyCombs = value;
                    OnPropertyChanged();
                    KeySeqStr = KeyCombSequence.KeyCombString();
                }
            } }

        private KeyCombSequence? keyCombSequence;
        public KeyCombSequence? KeyCombSequence { 
            get { return keyCombSequence; } 
            set { keyCombSequence = value; if (keyCombSequence != null) { KeySeqStr = keyCombSequence.KeyCombString(); } } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }


    /// <summary>
    /// Interaction logic for KeysConfWindow.xaml
    /// </summary>
    public partial class KeysConfWindow : Window
    {

        //private ObservableCollection<ListBoxItem> keySeqsLBoxes = new ObservableCollection<ListBoxItem>();
        //private ObservableCollection<ShortCutListViewModel> keySeqsLBoxes = new ObservableCollection<ShortCutListViewModel>();
        private ICollectionView filteredSeqs;

        public KeysConfWindow(KeySequenceMngr keySequenceMngr) : this()
        {
            KeySequenceMngr = keySequenceMngr;
            //KeySequenceMngr.StateManager = new KeySeqStateManager(KeySequenceMngr.DEFAULT_STATE_FILE_NAME);
            //KeySequenceMngr?.StateManager.LoadState(KeySequenceMngr);
            InitListBox();
            SearchBox.Focus();
        }


        public KeysConfWindow()
        {
            EnteredKeyComb = new List<KeyComb>();
            InitializeComponent();

            KeysConfWndwViewModel = new KeysConfWindowViewModel();
            //scw_vm.ShortcutVMs = new SCWindowViewModel();
            //DataContext = new SCWindowViewModel();
            DataContext = KeysConfWndwViewModel;

            this.PreviewMouseDown += WindowEventHandler;
            this.PreviewKeyDown += WindowEventHandler;
            //this.AddHandler(Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(WindowEventHandler), true);
            //this.AddHandler(Keyboard.PreviewKeyDownEvent, new KeyboardEventHandler(WindowEventHandler), true);

            //InitListBox();

            this.Closed += (s, e) => {
                //var it = ActionList.Items;
                //var it = keySeqsLBoxes;
                var it = KeysConfWndwViewModel.ShortcutVMs;
                List<KeyCombSequence> keyCombSeqs = new List<KeyCombSequence>();

                //foreach (ListBoxItem item in it) {
                foreach (ShortCutListViewModel item in it)
                {
                    //var kcs = (KeyCombSequence)item.Tag;
                    var kcs = item.KeyCombSequence;
                    keyCombSeqs.Add(kcs);
                }
                KeySequenceMngr.KeyCombSequences = keyCombSeqs;
                //KeySequenceMngr?.StateManager.SaveState(KeySequenceMngr);
                KeySequenceMngr?.SaveState();
            };
        }


        //MouseButtonEventArgs 
        private void WindowEventHandler(object sender, RoutedEventArgs e)
        {
            //MouseButtonEventArgs args;
            //args.OriginalSource
            //e.OriginalSource;

            if (e.OriginalSource is Button btn) {
                switch (btn.Name)
                {
                    case "AssignBtn": HandleAssignEvent(sender, e); break;
                    case "ClearBtn":
                        break;
                    default:
                        break;
                }
                //== "AssignBtn"
            }
            if (e is KeyEventArgs kea) {
                if (kea.Key == Key.Escape) { e.Handled = true; this.Close(); }
            }
        }

        

        private void HandleAssignEvent(object s, EventArgs e)
        {
            if (KeysConfWndwViewModel.SelectedShortcut == null) {
                MessageBox.Show("Select shortcut you want to assign new shortcut.");
                return;
            }
            if (EnteredKeyComb.Count == 0) {
                MessageBox.Show("Select shortcut keys you want to assign for the shortcut.");
                return;
            }
            KeysConfWndwViewModel.SelectedShortcut.KeyCombs  = EnteredKeyComb;
        }

        private void InitListBox()
        {
            try
            {
                if (KeySequenceMngr == null) return;
                //KeySequenceMngr.KeyCombSequences.ToList().ForEach(ks =>
                KeySequenceMngr.KeyCombSequences.ForEach(ks =>
                {
                    //var lbi = new ListBoxItem() { Content = ks.Name, Tag = ks };
                    //lbi.Selected += HandleSelectedItem;

                    var sclv = new ShortCutListViewModel() { KeyCombSequence = ks };
                    //keySeqsLBoxes.Add(sclv /*lbi*/);
                    KeysConfWndwViewModel.ShortcutVMs.Add(sclv);
                    //ActionList.Items.Add(lbi);
                });
                //filteredSeqs = CollectionViewSource.GetDefaultView(keySeqsLBoxes);
                filteredSeqs = CollectionViewSource.GetDefaultView(KeysConfWndwViewModel.ShortcutVMs);
                //ActionList.ItemsSource = keySeqsLBoxes;
                filteredSeqs.Filter = FilterSeqs;
                
                ActionList.ItemsSource = filteredSeqs;

                SearchBox.TextChanged += (s, e) => filteredSeqs.Refresh();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine( e);
                //throw;
            }            
            //ActionList.Items.Refresh();
        }

        private bool FilterSeqs(object obj)
        {
            if (obj == null) return false;
            //if (obj is ListBoxItem lbi)
            if (obj is ShortCutListViewModel lbi)
            {
                //KeyCombSequence keySeq = lbi.Tag as KeyCombSequence;
                KeyCombSequence? keySeq = lbi.KeyCombSequence;
                string filter = SearchBox.Text?.ToLower() ?? "";
                if (string.IsNullOrWhiteSpace(filter)) return true; // <-- Show all if no text
                if (keySeq != null) { 
                    return keySeq.Name.ToLower().Contains(filter);
                }
            }    
            return false;
        }

        private void HandleSelectedItem(object s, RoutedEventArgs e)
        {
            var item = s as ListBoxItem;
            SelectedItem = s;
            if (SelectedItem != null) { 
                var keyCombSeq = item.Tag as KeyCombSequence;
                var keyStr = keyCombSeq.KeyCombString();
            
                if (InfoTextBlock != null) {
                    //InfoTextBlock.Text = $"Name: {keyCombSeq.Name}\nShortcut: {keyStr}\n\nDescription:\n";
                    InfoTextBlock.Text = $"Current key sequence = {keyStr}";
                }
            }
        }

        private void ActionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ks = ActionList.SelectedItem as KeyCombSequence;
            //ks.KeyCombs
        }

        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void AssignKey_Click(object sender, RoutedEventArgs e)
        {

        }

        public List<KeyComb> EnteredKeyComb { get; set; }
        public KeySequenceMngr KeySequenceMngr { get; }
        public object SelectedItem { get; private set; }
        public KeysConfWindowViewModel KeysConfWndwViewModel { get; }

        private void ShortcutBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var mods = Keyboard.Modifiers;

            List<Key> specialKeys = new List<Key>() { Key.LeftAlt, Key.LeftCtrl, Key.LeftShift, Key.RightAlt, Key.RightCtrl, Key.RightShift };

            if (e.Key != Key.Tab && !specialKeys.Contains(e.Key))
            {
                e.Handled = true;  // Prevents TextBox from typing characters
                EnteredKeyComb.Add(new KeyComb { Key = e.Key, Mods = mods });

                ShortcutBox.Text = GetShortcutString();
                CheckOtherShortCutsForDuplicates();
            }
        }

        private void CheckOtherShortCutsForDuplicates()
        {
            var compareResults = KeySequenceMngr.KeyCombSequences.Select(ks => (ks: ks, comp: ks.Compare(EnteredKeyComb)));
            var equals     = compareResults.Where(r => r.comp == CompareResult.Equivalent);
            var startsWith = compareResults.Where(r => r.comp == CompareResult.StartsWith);
            var overlaps   = compareResults.Where(r => r.comp == CompareResult.Overlaps);

            //var equalComb = KeySequenceMngr.KeyCombSequences.FirstOrDefault(ks => ks.Compare(EnteredKeyComb));
            if (equals.Count() > 0)
            {
                KeysConfWndwViewModel.NotifStr = $"Already assigned to {equals.FirstOrDefault().ks.Name}";
            }
            else if (startsWith.Count() > 0)
            {
                KeysConfWndwViewModel.NotifStr = $"Is mixed with {startsWith.FirstOrDefault().ks.Name}";
            }
            else if (overlaps.Count() > 0)
            {
                KeysConfWndwViewModel.NotifStr = $"Is mixed with {overlaps.FirstOrDefault().ks.Name}";
            }
            else {
                KeysConfWndwViewModel.NotifStr = "";
            }

        }

        private void ShortcutBox_KeyUp(object sender, KeyEventArgs e)
        {
            //// Allow future updates
            //_isUpdating = false;

            //// Update display when modifiers are released
            //ShortcutBox.Text = GetShortcutString(null);
        }

        private string GetShortcutString() {
            return string.Join(", ", EnteredKeyComb.Select(s=>KeyCombToSring(s)));
        }

        private string KeyCombToSring(KeyComb ekc)
        {
            var list = new List<string>() {
                (ekc.Mods & ModifierKeys.Alt    ) == ModifierKeys.Alt     ? "Alt"   : "",
                (ekc.Mods & ModifierKeys.Control) == ModifierKeys.Control ? "Ctrl"  : "",
                (ekc.Mods & ModifierKeys.Shift  ) == ModifierKeys.Shift   ? "Shift" : "",
                (ekc.Mods & ModifierKeys.Windows) == ModifierKeys.Windows ? "Win"   : ""
            };
            list.RemoveAll(s => s == "");

            var modsStr = string.Join(" + ", list);

            //var modsList = new List<ModifierKeys>() { } ;
            //var modsStr = string.Join(" + ", modsList.Select(m => ModKeyToStr(m)));

            //return $"{modsStr} + {ekc.Key.ToString()}";
            if (list.Count > 0)
            {
                return $"{modsStr} + {ekc.Key.ToString()}";
            }
            return ekc.Key.ToString();
        }

        private void ShortcutBox_KeyDown(object sender, KeyEventArgs e)
        {
            //// Check if Ctrl or Alt is pressed, if is check e.Key digits and letters
            //if (Keyboard.Modifiers == ModifierKeys.Control || Keyboard.Modifiers == ModifierKeys.Alt) 
            //{
            //    System.Diagnostics.Debug.WriteLine( e.Key);
            //    // Check if e.Key is digit or letter
            //    if ((e.Key >= Key.A && e.Key <= Key.Z) || (e.Key >= Key.D0 && e.Key <= Key.D9)) 
            //    {
            //        e.Handled = true;  // Prevents TextBox from typing characters
            //        // Avoid repeated updates while holding a key
            //        //if (_isUpdating) return;
            //        //_isUpdating = true;
            //        ShortcutBox.Text = GetShortcutString(e);
            //    }
            //}
        }

        private void ClearKey_Click(object sender, RoutedEventArgs e)
        {
            this.EnteredKeyComb.Clear();
            ShortcutBox.Text = "Press shortcut...";
            KeysConfWndwViewModel.NotifStr = "";
        }

        //private void SearchTextChanged(object sender, TextChangedEventArgs e)
        //{
        //    var searchStr = SearchBox.Text;
        //    ActionList.Items.Clear();

        //    if (KeySequenceMngr == null) return;
        //    KeySequenceMngr.KeyCombSequences.Where(s=>s.Name.Contains(searchStr)).ToList().ForEach(ks =>
        //    {
        //        //ActionList.Items.Add(ks);
        //        var lbi = new ListBoxItem() { Content = ks.Name, Tag = ks };
        //        //lbi.Selected += (s, e) => { HandleSelectedItem(s, e); };
        //        lbi.Selected += HandleSelectedItem;
        //        ActionList.Items.Add(lbi);
        //    });
        //}

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            var header = mi?.Header?.ToString().Replace("_", "");
            switch (header)
            {
                case "Save as":
                    FileStyleUriParser  fsup = new FileStyleUriParser();
                    var sfd = new Microsoft.Win32.SaveFileDialog()
                    {
                        DefaultExt = ".json",
                        Filter = "Json files (*.json)|*.json",
                        Title = "Save Shortcut Keys Configuration"
                    };
                    bool? result = sfd.ShowDialog();
                    if (result == true)
                    {
                        string filename = sfd.FileName;
                        KeySequenceMngr.SaveToFile(filename);
                    }
                    break;
                case "Open":
                    var ofd = new Microsoft.Win32.OpenFileDialog()
                    {
                        DefaultExt = ".json",
                        Filter = "Json files (*.json)|*.json",
                        Title = "Open Shortcut Keys Configuration"
                    };
                    bool? openResult = ofd.ShowDialog();
                    if (openResult == true)
                    {
                        string filename = ofd.FileName;
                        KeySequenceMngr.LoadFromFile(filename);
                        //MessageBox.Show("Not implemented yet.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    break;
                case "Help":
                    string helpMsg = "To assign a new shortcut:\n" +
                                     "1. Select the shortcut from the list.\n" +
                                     "2. Click on the 'Shortcut' box and press the desired key combination.\n" +
                                     "3. Click the 'Assign' button to save the new shortcut.\n\n" +
                                     "To clear the current key combination, click the 'Clear' button.\n\n" +
                                     "Use the search box to filter shortcuts by name.";
                    MessageBox.Show(helpMsg, "Shortcut Keys Configuration Help", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                default:
                    break;
            }
        }

        
    }
}
