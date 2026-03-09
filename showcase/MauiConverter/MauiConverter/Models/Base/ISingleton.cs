namespace MauiConverter;

interface ISingleton<T>
{
	abstract static T Instance { get; }
}