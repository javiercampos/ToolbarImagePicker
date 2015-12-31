using System;
using System.ComponentModel;
using System.Reflection;
using EnvDTE;

namespace Jcl.Tools.WindowsForms.ToolbarImagePicker
{
	public static class DefaultResourcePickerPlug
	{
		private static Type _resourcePickerDialogType;
		private static Action _okAction;
		private static Action _cancelAction;

		public static void SetOkAction(Action act)
		{
			_okAction = act;
		}

		public static void SetCancelAction(Action act)
		{
			_cancelAction = act;
		}

		
		public static dynamic GetPickerForm(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			var path = AssemblyTools.GetAssemblyPath(@"Microsoft.VisualStudio.Windows.Forms");
			var ass = Assembly.LoadFrom(path);

			_resourcePickerDialogType = ass.GetType(@"Microsoft.VisualStudio.Windows.Forms.ResourcePickerDialog+ResourcePickerUI");
			dynamic resourcePickerDialogint = Activator.CreateInstance(_resourcePickerDialogType);

			var resourcePickerServiceType = ass.GetType(@"Microsoft.VisualStudio.Windows.Forms.ResourcePickerService");
			dynamic resourcePickerService = Activator.CreateInstance(resourcePickerServiceType, 
																																BindingFlags.NonPublic | BindingFlags.Instance, 
																																null, 
																																new object [] { (_DTE) provider.GetService(typeof (_DTE)), provider },
																																null);


			dynamic resourcePickerDialog = ExposedObject.From(resourcePickerDialogint);

			resourcePickerDialog.Start(value, context?.PropertyDescriptor?.PropertyType, resourcePickerService, provider);

			Action<object, object> okAction = (o, e) =>
			{
				_okAction?.Invoke();
			};
			Action<object, object> cancelAction = (o, e) =>
			{
				_cancelAction?.Invoke();
			};

			var okButton = resourcePickerDialog.okButton;
			var ei = okButton.GetType().GetEvent("Click");
			var okDelegate = Delegate.CreateDelegate(ei.EventHandlerType, okAction.Target, okAction.Method);
			ei.AddEventHandler(okButton, okDelegate);

			var cancelButton = resourcePickerDialog.cancelButton;
			ei = cancelButton.GetType().GetEvent("Click");
			var cancelDelegate = Delegate.CreateDelegate(ei.EventHandlerType, cancelAction.Target, cancelAction.Method);
			ei.AddEventHandler(cancelButton, cancelDelegate);

			return resourcePickerDialog;
		}

		public static object GetPickerValue(dynamic resourcePickerDialog)
		{
			return resourcePickerDialog.EditValue;
		}
		public static void EndPickerForm(dynamic resourcePickerDialog)
		{
			var dynMethod = _resourcePickerDialogType.GetMethod("End", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			dynMethod.Invoke(resourcePickerDialog.Object, null);
		}
	}
}