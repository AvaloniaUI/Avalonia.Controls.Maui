namespace MauiConverter;

abstract class BaseContentPage<TViewModel> : ContentPage where TViewModel : BaseViewModel
{
	protected BaseContentPage(TViewModel viewModel) => base.BindingContext = viewModel;

	protected new TViewModel BindingContext => (TViewModel)base.BindingContext;
}