using AlohaKit.Gallery.Models;
using System.Windows.Input;

// View types for AOT page factory
using AlohaKit.Gallery;

namespace AlohaKit.Gallery.Views.Base
{
	public class BasePage : ContentPage
	{
		SectionModel? _selectedItem;

		// Static page factory for AOT compatibility - no reflection
		private static readonly Dictionary<Type, Func<Page>> PageFactory = new()
		{
			[typeof(AvatarView)] = () => new AvatarView(),
			[typeof(LoadingView)] = () => new LoadingView(),
			[typeof(ButtonView)] = () => new ButtonView(),
			[typeof(CaptchaView)] = () => new CaptchaView(),
			[typeof(CheckBoxView)] = () => new CheckBoxView(),
			[typeof(LinearGaugeView)] = () => new LinearGaugeView(),
			[typeof(NumericUpDownView)] = () => new NumericUpDownView(),
			[typeof(PieChartView)] = () => new PieChartView(),
			[typeof(PulseIconView)] = () => new PulseIconView(),
			[typeof(ProgressBarView)] = () => new ProgressBarView(),
			[typeof(ProgressRadialView)] = () => new ProgressRadialView(),
			[typeof(SegmentedControlView)] = () => new SegmentedControlView(),
			[typeof(RatingView)] = () => new RatingView(),
			[typeof(SliderView)] = () => new SliderView(),
			[typeof(ToggleSwitchView)] = () => new ToggleSwitchView(),
			[typeof(BarChartView)] = () => new BarChartView(),
			[typeof(LineChartView)] = () => new LineChartView(),
			[typeof(MultiBarChartView)] = () => new MultiBarChartView(),
			[typeof(MultiLineChartView)] = () => new MultiLineChartView(),
		};

		public BasePage()
		{
			NavigateCommand = new Command(async () =>
			{
				if (SelectedItem != null)
				{
					await Navigation.PushAsync(PreparePage(SelectedItem));

					SelectedItem = null;
				}
			});
		}

		public ICommand NavigateCommand { get; }

		public SectionModel? SelectedItem
		{
			get { return _selectedItem; }
			set
			{
				_selectedItem = value;
				OnPropertyChanged();
			}
		}

		Page PreparePage(SectionModel model)
		{
			var page = (Handler?.MauiContext?.Services?.GetService(model.Type) as Page)
				?? (PageFactory.TryGetValue(model.Type, out var factory) ? factory() : throw new InvalidOperationException($"No factory registered for {model.Type}"));
			page.Title = model.Title;

			return page;
		}
	}
}