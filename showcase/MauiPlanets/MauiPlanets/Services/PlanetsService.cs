namespace MauiPlanets.Services;

internal static class PlanetsService
{
	private static  List<Planet> planets = new()
	{
		new()
		{
			Name = "Mercury",
			Subtitle = "The smallest planet",
			HeroImage = "mercury.png",
			Description = "Mercury is the first of the four terrestrial planets. This means it is a planet made mostly of rock. The planets closest to the Sun—Venus, Earth, and Mars—are the other three.",
			AccentColorStart = Color.FromArgb("#353535"),
			AccentColorEnd = Color.FromArgb("#8d9098"),
			Images = new()
            {
                "mercury_1.jpg",
                "mercury_2.jpg",
                "mercury_3.jpg"
            }
		},
        new()
        {
            Name = "Venus",
            Subtitle = "The pressure cooker",
            HeroImage = "venus.png",
            Description = "Of all the planets, Venus is the one most similar to Earth. In fact, Venus is often called Earth's “sister” planet. As similar as it is in some ways, however, it is also very different in others.",
            AccentColorStart = Color.FromArgb("#a6393b"),
            AccentColorEnd = Color.FromArgb("#d17f21"),
            Images = new()
            {
                "venus_1.jpg",
                "venus_2.jpg",
                "venus_3.jpg",
                "venus_4.jpg"
            }
        },
        new()
        {
            Name = "Earth",
            Subtitle = "The cradle of life",
            HeroImage = "earth.png",
            Description = "The Earth is the only planet known where life exists. Almost 1.5 million species of animals and plants have been discovered so far, and many more have yet to be found. While other planets may have small amounts of ice or steam, the Earth is 2/3 water. Earth has perfect conditions for a breathable atmosphere.",
            AccentColorStart = Color.FromArgb("#0e3d68"),
            AccentColorEnd = Color.FromArgb("#2e97c7"),
            Images = new()
            {
                "earth_1.png",
                "earth_2.jpg",
                "earth_3.jpg"
            }
        },
        new()
        {
            Name = "Mars",
            Subtitle = "The red beauty",
            HeroImage = "mars.png",
            Description = "No planet has sparked the imaginations of humans as much as Mars. It may be the reddish color of Mars, or the fact that it can often be easily seen in the night sky, that has caused people to wonder about this close neighbor of ours. Tales of “Martians” invading Earth have been around for well over fifty years. But is it likely that any kind of life really does exist on Mars?",
            AccentColorStart = Color.FromArgb("#a23036"),
            AccentColorEnd = Color.FromArgb("#eb3333"),
            Images = new()
            {
                "mars_1.jpeg",
                "mars_2.jpg",
                "mars_3.jpg"
            }
        },
        new()
        {
            Name = "Jupiter",
            Subtitle = "The gas giant",
            HeroImage = "jupiter.png",
            Description = "The planet Jupiter is the first of the gas giant planets. Made mostly of gas, they include Jupiter, Saturn, Uranus, and Neptune.\n\nJupiter is first among the planets in terms of size and mass. Its diameter is 11 times bigger than Earth, and its mass is 2.5 times greater than all the other planets combined. The “Great Red Spot” on Jupiter is actually a raging storm.",
            AccentColorStart = Color.FromArgb("#9d4a40"),
            AccentColorEnd = Color.FromArgb("#cd8026"),
            Images = new()
            {
                "jupiter_1.jpg",
                "jupiter_2.jpg",
                "jupiter_3.jpg"
            }
        },
        new()
        {
            Name = "Saturn",
            Subtitle = "The ring planet",
            HeroImage = "saturn.png",
            Description = "Most people know about the rings around Saturn, because they are the brightest and most colorful. These rings are made mainly out of ice particles orbiting the planet. While the rings themselves seem big, the particles are very small, usually no more than 10 feet (3 meters) wide.",
            AccentColorStart = Color.FromArgb("#996237"),
            AccentColorEnd = Color.FromArgb("#c6502f"),
            Images = new()
            {
                "saturn_1.jpg",
                "saturn_2.jpg",
                "saturn_3.jpg",
                "saturn_4.jpg"
            }
        },
        new()
        {
            Name = "Uranus",
            Subtitle = "The cold ball",
            HeroImage = "uranus.png",
            Description = "Uranus is the first planet so far away from the Earth that it can only be seen with the use of a telescope. When it was first discovered in 1781, scientists didn't know what they had found. As astronomers studied the object more closely, they discovered that it had a circular orbit around the Sun. They had found the seventh planet.",
            AccentColorStart = Color.FromArgb("#9d4a40"),
            AccentColorEnd = Color.FromArgb("#996237"),
            Images = new()
            {
                "uranus_1.jpg",
                "uranus_2.jpg",
                "uranus_.jpg"
            }
        },
        new()
        {
            Name = "Neptune",
            Subtitle = "Eighth & fathest-away",
            HeroImage = "neptune.png",
            Description = "Imagine being so good at math that you could figure out the location of a planet you had never even seen! That is what John C. Adams did in 1843 when he discovered Neptune.",
            AccentColorStart = Color.FromArgb("#0c293d"),
            AccentColorEnd = Color.FromArgb("#26abe0"),
            Images = new()
            {
                "neptune_1.jpg",
                "neptune_2.jpg",
                "neptune_3.jpg"
            }
        }
    };

	public static List<Planet> GetAllPlanets()
		=> planets;

    public static Planet? GetPlanet(string planetName)
		=> planets.Where(_planet => _planet.Name == planetName).FirstOrDefault();

    public static List<Planet> GetFeaturedPlanets()
    {
        var rnd = new Random();
        var randomizedPlanets = planets.OrderBy(item => rnd.Next());

		return randomizedPlanets.Take(2).ToList();
    }
        

}

