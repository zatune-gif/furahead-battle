using UnityEngine.SceneManagement;

public static class SceneTransition
{
    public static void GoToTitle() => SceneManager.LoadScene("TitleScene");
    public static void GoToCharacterSelect() => SceneManager.LoadScene("CharacterSelectScene");
    public static void GoToBattle() => SceneManager.LoadScene("BattleScene");
}
