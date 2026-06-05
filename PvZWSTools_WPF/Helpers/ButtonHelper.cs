namespace PvZWSTools_WPF.Helpers
{
    public static class ButtonHelper
    {
        public static string GetCheckValue(string symbol) =>
            symbol == Constants.c_Symbol_On ? Constants.c_Value_Checked :
            symbol == Constants.c_Symbol_Off ? Constants.c_Value_Unchecked : Constants.c_Value_Error;

        public static string ToggleChallenge(string current) =>
            current switch
            {
                "0" => "1",
                "1" => "2",
                "2" => "0",
                _ => current
            };

        public static string ToggleCheck(string current) =>
            current switch
            {
                Constants.c_Symbol_On => Constants.c_Symbol_Off,
                Constants.c_Symbol_Off => Constants.c_Symbol_On,
                Constants.c_Symbol_UnKnown => Constants.c_Symbol_On,
                _ => Constants.c_Symbol_UnKnown
            };
    }
}
