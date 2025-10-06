namespace ArchipelagoRandomizer;

internal class NotifyAndSave {
    public static void Default(GameFlagDescriptable gfd, int count, int oldCount) {
        if (gfd == null || count <= oldCount)
            return;

        // The important parts of ItemGetUIShowAction.Implement() for our purposes are ShowGetDescriptablePrompt()/ShowDescriptableNitification() and AutoSave().
        // Prompt() shows the large blocking popup, while Nitification() shows the tiny message in the corner.

        // Turns out ShowGetDescriptablePrompt() is prone to native Unity crashes. It's most consistent when called
        // immediately after an AP location check + item receipt for an important item like Mystic Nymph.
        // Unfortunately a delayed call is even more crash-y, often crashing just on toggling nymph without any AP networking.
        // So we have to use ShowDescriptableNitification() on all items. Fortunately, players seem to prefer this anyway.
        // If we ever use ShowGetDescriptablePrompt() again, note that it will display "No Data" unless the gfd has already been applied.
        SingletonBehaviour<UIManager>.Instance.ShowDescriptableNitification(gfd);

        SingletonBehaviour<SaveManager>.Instance.AutoSave(SaveManager.SaveSceneScheme.FlagOnly, forceShowIcon: true);
    }

    // Sometimes we want to show a notification for an item we invented for AP, using custom text and one of the vanilla game's sprites.
    public static void WithCustomText(GameFlagDescriptable gfd, string customText, int count, int oldCount) {
        if (gfd == null || count <= oldCount)
            return;

        // Normally ShowDescriptableNitification() calls notificationUI.ShowNotification() after figuring out values for text and targetPanel.
        // For a "fake" item, we define the text, and we want targetPanel's default of Unknown, so there's no downside to calling ShowNotification() directly.
        SingletonBehaviour<GameCore>.Instance.notificationUI.ShowNotification(gfd, customText, gfd.spriteRef);

        SingletonBehaviour<SaveManager>.Instance.AutoSave(SaveManager.SaveSceneScheme.FlagOnly, forceShowIcon: true);
    }
}
