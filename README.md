# ShortCutKeysLib
## Shortcut key library for WPF applications

Allows user to configure shortcut keys defined in the application.

![Keys configuration UI](https://github.com/mattire/ShortCutKeysLib/raw/main/images/KeysConfSamplePic.png)

Demo [https://youtu.be/dYdjHuFCk9Q](https://youtu.be/dYdjHuFCk9Q)

Defining default shortcuts:

New syntax (version 1.0.1):

>       public ShortCutKeysLib.KeySequenceMngr ShortCutKeysMngr = new ShortCutKeysLib.KeySequenceMngr(
>           keyCombSeqs: new List<ShortCutKeysLib.KeyCombSequence>()
>           {
>               new ("OpenGoogleSearchLink", "Opens Google search link for the selected stock symbol.", // name and description
>			(e,s) => { OpenLink(GoogleSearchLink); }, // action
>			MW.SearchBox, // UI element where shortcut key is active, if null, active everywhere around the window
>			Key.G, Key.D2), // keys interpreted as control keys: ctrl + G, ctrl + 2
>			
>			new ("Open.TradingView.Nasdaq"   , NoDesc, 
>			(e, s) => { OpenLink(TradingViewNasdaqLink); e.Handled = true; },
>			null, 
>			(Key.T, ModifierKeys.Control), (Key.T, ModifierKeys.Control | ModifierKeys.Shift)), // pairs describing keys and their modifier keys
>		)

Old outdated syntax (version 1.0.0):

>        public ShortCutKeysLib.KeySequenceMngr ShortCutKeysMngr = new ShortCutKeysLib.KeySequenceMngr(
>            controlKeyShortCuts: new List<(List<Key> keys, Action<KeyEventArgs, object> act, object? sender, string name, string desc)>()
>            {
>                ( new List<Key>() { Key.G, Key.D2 }, (e,s) => { OpenLink(GoogleSearchLink);        }, MW.SearchBox, "OpenGoogleSearchLink", "Opens Google search link for the selected stock symbol." ),
>            },
>            keyCombSeqs: new List<ShortCutKeysLib.KeySequenceMngr.KeyCombSequence>()
>            {
>                // Define key combinations and their actions here
>                new ShortCutKeysLib.KeySequenceMngr.KeyCombSequence("SearchBox.ListMoveDown", "Stock list move down",
>                    new List<KeyComb>(){ new KeyComb() { Key = Key.Down, Mods = ModifierKeys.None } },
>                    (e, s) => { MW.MoveSelection(1); e.Handled = true; }, MW.SearchBox),
>                new ShortCutKeysLib.KeySequenceMngr.KeyCombSequence("SearchBox.ListMoveUp", "Stock list move up",
>                    new List<KeyComb>(){ new KeyComb() { Key = Key.Up, Mods = ModifierKeys.None } },
>                    (e, s) => { MW.MoveSelection(-1); e.Handled = true; }, MW.SearchBox),
>            },
>            confFileName: "mainWindowShorts.json"
>        );

Hooking up keypresses to shortcut handler:

>		this.KeyDown += Utils.KeyBoardHelper.Instance.MainWindow_KeyDown;
>		this.PreviewKeyDown += Utils.KeyBoardHelper.Instance.MainWindow_KeyDown;
>
>		public void MainWindow_KeyDown(object sender, KeyEventArgs e)
>		{
>			ShortCutKeysMngr.CheckSequences(e, sender);
>		}

When the user changes the shortcuts, they are stored into mainWindowShorts.json in the above example. 
It is possible to tie the shortcut to specific UI element. In the above example shortcuts apply to keypress in the MW.SearchBox UI object.

Showing shortcuts editing window to user:

>       private void Test_Click(object sender, RoutedEventArgs e)
>       {   
>           new ShortCutKeysLib.KeysConfWindow(ShortCutKeysMngr).Show();
>       }



Source codes of example app will be added some day.
