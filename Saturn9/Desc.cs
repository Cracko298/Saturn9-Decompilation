namespace Saturn9;

public class Desc
{
	public int m_Value;

	public string m_Text;

	public bool m_UseYesNo;

	public int m_Min;

	public int m_Max;

	public bool m_HasValue;

	public Desc()
	{
		m_Value = 0;
		m_Text = "";
		m_UseYesNo = false;
		m_Min = 0;
		m_Max = 1;
		m_HasValue = true;
	}
}
