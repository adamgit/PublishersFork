using UnityEngine.UIElements;

namespace PublishersFork
{
	/// <summary>
	/// The CSS Specification has for 30 years allowed you to set the margins with shorthands for
	/// "all margins", "top and bottom only", and "left and right only". Unity refuses to follow
	/// the specification here. These methods bring Unity closer to the specification.
	/// </summary>
	public static class WorkaroundUnityUIToolkitMarginsAll
	{
		public static void MarginsLeftRight( this IStyle s, StyleLength len )
		{
			s.marginLeft = s.marginRight = len;
		}
    
		public static void MarginsTopBottom( this IStyle s, StyleLength len )
		{
			s.marginTop = s.marginBottom = len;
		}
    
		public static void Margins( this IStyle s, StyleLength len )
		{
			s.MarginsLeftRight( len );
			s.MarginsTopBottom( len );
		}
	}
}