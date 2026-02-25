using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries.ScrollModeGalleries
{
	class EnumPicker : Picker
	{
		public static readonly BindableProperty EnumTypeProperty =
			BindableProperty.Create(nameof(EnumType), typeof(Type), typeof(EnumPicker),
				propertyChanged: (bindable, oldValue, newValue) =>
				{
					EnumPicker picker = (EnumPicker)bindable;

					if (oldValue != null)
					{
						picker.ItemsSource = null;
					}
					if (newValue != null)
					{
						if (!((Type)newValue).GetTypeInfo().IsEnum)
							throw new ArgumentException("EnumPicker: EnumType property must be enumeration type");

						var enumType = (Type)newValue;
						var underlyingValues = Enum.GetValuesAsUnderlyingType(enumType);
						var names = new List<string>(underlyingValues.Length);
						foreach (var value in underlyingValues)
						{
							names.Add(Enum.GetName(enumType, value)!);
						}
						picker.ItemsSource = names;
					}
				});

		public Type EnumType
		{
			set => SetValue(EnumTypeProperty, value);
			get => (Type)GetValue(EnumTypeProperty);
		}
	}
}
