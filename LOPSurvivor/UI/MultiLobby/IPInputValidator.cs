using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IPInputValidator : MonoBehaviour
{
    [Header("ServerConnect InputField")]
    [SerializeField] private TMP_InputField ip_InputField;
    [SerializeField] private TMP_InputField port_InputField;
    [SerializeField] private TMP_InputField nickname_InputField;

    [SerializeField] private Button serverConnect_btn;

    private const int min_txt = 4;
    private const int max_txt = 16;

    private const int nickname_min_txt = 1;
    private const int nickname_max_txt = 8;

    public bool isIpValid;
    public bool isPortValid;
    public bool isNicknameValid;

    private void Start()
    {
        // 입력 제한
        ip_InputField.onValidateInput += ValidateIp;
        port_InputField.onValidateInput += ValidatePort;
        nickname_InputField.onValidateInput += ValidateNickname;

        // 값 변경 감지
        ip_InputField.onValueChanged.AddListener(
            text => OnFieldChanged(ip_InputField));

        port_InputField.onValueChanged.AddListener(
            text => OnFieldChanged(port_InputField));

        nickname_InputField.onValueChanged.AddListener(
            text => OnFieldChanged(nickname_InputField));
    }

    private char ValidateIp(string text, int index, char addedChar)
    {
        if (char.IsDigit(addedChar) || addedChar == '.')
            return addedChar;

        return '\0';
    }

    private char ValidatePort(string text, int index, char addedChar)
    {
        if (char.IsDigit(addedChar))
            return addedChar;

        return '\0';
    }

    private char ValidateNickname(string text, int index, char addedChar)
    {
        // 한글, 영문, 숫자만 허용
        if (char.IsLetterOrDigit(addedChar) ||
            (addedChar >= '가' && addedChar <= '힣')) // -> 한글은 처음 해봐서 찾아봄
        {
            return addedChar;
        }
        return '\0';
    }

    private void OnFieldChanged(TMP_InputField field)
    {
        // 공백 제거
        string cleanText = field.text.Replace(" ", "");

        // 최대 길이 결정
        int maxLength = max_txt;

        if (field == nickname_InputField) maxLength = nickname_max_txt;

        // 최대 길이 초과 시 컷
        if (cleanText.Length > maxLength) cleanText = cleanText.Substring(0, maxLength);

        if (cleanText != field.text)
        {
            field.text = cleanText;
            field.caretPosition = cleanText.Length;
        }

        // 길이 유효성 검사
        bool isValidLength;

        if (field == nickname_InputField)
            isValidLength = field.text.Length >= nickname_min_txt && field.text.Length <= nickname_max_txt;
        else
            isValidLength = field.text.Length >= min_txt && field.text.Length <= max_txt;

        field.textComponent.color = isValidLength ? Color.black : Color.red;

        InputFieldState();
    }

    private bool IsLengthValid(string text)
    {
        int length = text.Length;
        return length >= min_txt && length <= max_txt;
    }

    public void InputFieldState()
    {
        isIpValid = ip_InputField.text.Length >= min_txt && ip_InputField.text.Length <= max_txt;

        isPortValid = port_InputField.text.Length >= min_txt && port_InputField.text.Length <= max_txt;

        isNicknameValid = nickname_InputField.text.Length >= nickname_min_txt && nickname_InputField.text.Length <= nickname_max_txt;

        if (isIpValid && isPortValid && isNicknameValid) serverConnect_btn.interactable = true;
        else serverConnect_btn.interactable = false;
    }
}
