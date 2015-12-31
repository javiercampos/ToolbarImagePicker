using System.Dynamic;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace Jcl.Tools.WindowsForms.ToolbarImagePicker
{
	internal class LocalizedStringResource : DynamicObject
	{
		private readonly ResourceManager _currentManager;

		public LocalizedStringResource()
		{
			var culture = CultureInfo.CurrentCulture;
			if (culture.TwoLetterISOLanguageName?.ToLower() == "es")
				_currentManager = LocalizedStrings_Spanish.ResourceManager;
			else
				_currentManager = LocalizedStrings_English.ResourceManager;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			try
			{
				result = _currentManager?.GetString(binder.Name, null);
				return true;
			}
			catch
			{
				result = null;
			}
			return base.TryGetMember(binder, out result);
		}
	}


}