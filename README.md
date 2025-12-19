# ShortCutKeysLib
## Shortcut key library for WPF applications

Allows user to configure shortcut keys defined in the application.

![Keys configuration UI](https://github.com/mattire/ShortCutKeysLib/raw/main/images/KeysConfSamplePic.png)

Demo [https://youtu.be/dYdjHuFCk9Q](https://youtu.be/dYdjHuFCk9Q)

Defining default shortcuts:

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

Source codes of example app will be added some day.
