using System.Collections.ObjectModel;

namespace ControlGallery.Pages.ShellSamples;

public static class AnimalData
{
    public static IList<Animal> Monkeys { get; private set; } = new ObservableCollection<Animal>();
    public static IList<Animal> Cats { get; private set; } = new ObservableCollection<Animal>();
    public static IList<Animal> Dogs { get; private set; } = new ObservableCollection<Animal>();
    public static IList<Animal> Bears { get; private set; } = new ObservableCollection<Animal>();
    public static IList<Animal> Elephants { get; private set; } = new ObservableCollection<Animal>();
    public static IList<Animal> AllAnimals { get; private set; } = new List<Animal>();

    static AnimalData()
    {
        Monkeys = new ObservableCollection<Animal>
        {
            new Animal
            {
                Name = "Baboon",
                Location = "Africa & Asia",
                Details = "Baboons are African and Arabian Old World monkeys belonging to the genus Papio, part of the subfamily Cercopithecinae.",
                ImageUrl = "baboon.jpg"
            },
            new Animal
            {
                Name = "Capuchin Monkey",
                Location = "Central & South America",
                Details = "The capuchin monkeys are New World monkeys of the subfamily Cebinae. Prior to 2011, the subfamily contained only a single genus, Cebus.",
                ImageUrl = "capuchin.jpg"
            },
            new Animal
            {
                Name = "Blue Monkey",
                Location = "Central and East Africa",
                Details = "The blue monkey or diademed monkey is a species of Old World monkey native to Central and East Africa, ranging from the upper Congo River basin east to the East African Rift and south to northern Angola and Zambia.",
                ImageUrl = "bluemonkey.jpg"
            },
            new Animal
            {
                Name = "Squirrel Monkey",
                Location = "Central & South America",
                Details = "The squirrel monkeys are the New World monkeys of the genus Saimiri. They are the only genus in the subfamily Saimirinae.",
                ImageUrl = "squirrelmonkey.jpg"
            },
            new Animal
            {
                Name = "Golden Lion Tamarin",
                Location = "Brazil",
                Details = "The golden lion tamarin also known as the golden marmoset, is a small New World monkey of the family Callitrichidae.",
                ImageUrl = "goldentamarin.jpg"
            }
        };

        Cats = new ObservableCollection<Animal>
        {
            new Animal
            {
                Name = "Abyssinian",
                Location = "Ethiopia",
                Details = "The Abyssinian is a breed of domestic short-haired cat with a distinctive ticked tabby coat, in which individual hairs are banded with different colors.",
                ImageUrl = "abyssinian.jpg"
            },
            new Animal
            {
                Name = "Bengal",
                Location = "Asia",
                Details = "The Bengal cat is a domesticated cat breed created from hybrids of domestic cats and the Asian leopard cat. Bengals have a wild appearance and may show spots, rosettes, arrowhead markings, or marbling.",
                ImageUrl = "bengal.jpg"
            },
            new Animal
            {
                Name = "Burmese",
                Location = "Thailand",
                Details = "The Burmese cat is a breed of domestic cat, originating in Thailand, believed to have its roots near the present Thai-Burma border.",
                ImageUrl = "burmese.jpg"
            },
            new Animal
            {
                Name = "Scottish Fold",
                Location = "Scotland",
                Details = "The Scottish Fold is a breed of domestic cat with a natural dominant-gene mutation that affects cartilage throughout the body, causing the ears to fold.",
                ImageUrl = "scottishfold.jpg"
            }
        };

        Dogs = new ObservableCollection<Animal>
        {
            new Animal
            {
                Name = "Afghan Hound",
                Location = "Afghanistan",
                Details = "The Afghan Hound is a hound that is distinguished by its thick, fine, silky coat and its tail with a ring curl at the end.",
                ImageUrl = "afghanhound.jpg"
            },
            new Animal
            {
                Name = "Bearded Collie",
                Location = "Scotland",
                Details = "The Bearded Collie, or Beardie, is a herding breed of dog once used primarily by Scottish shepherds, but now mostly a popular family companion.",
                ImageUrl = "beardedcollie.jpg"
            },
            new Animal
            {
                Name = "Boston Terrier",
                Location = "United States",
                Details = "The Boston Terrier is a breed of dog originating in the United States of America. This American Gentleman was accepted in 1893 by the American Kennel Club.",
                ImageUrl = "bostonterrier.jpg"
            },
            new Animal
            {
                Name = "Irish Terrier",
                Location = "Ireland",
                Details = "The Irish Terrier is a dog breed from Ireland, one of many breeds of terrier. The Irish Terrier is considered one of the oldest terrier breeds.",
                ImageUrl = "irishterrier.jpg"
            }
        };

        Bears = new ObservableCollection<Animal>
        {
            new Animal
            {
                Name = "American Black Bear",
                Location = "North America",
                Details = "The American black bear is a medium-sized bear native to North America. It is the continent's smallest and most widely distributed bear species.",
                ImageUrl = "blackbear.jpg"
            },
            new Animal
            {
                Name = "Brown Bear",
                Location = "Northern Eurasia & North America",
                Details = "The brown bear is a bear that is found across much of northern Eurasia and North America. In North America the population of brown bears are often called grizzly bears.",
                ImageUrl = "brownbear.jpg"
            },
            new Animal
            {
                Name = "Giant Panda",
                Location = "China",
                Details = "The giant panda, also known as panda bear or simply panda, is a bear native to south central China. It is easily recognized by the large, distinctive black patches around its eyes.",
                ImageUrl = "panda.jpg"
            },
            new Animal
            {
                Name = "Polar Bear",
                Location = "Arctic Circle",
                Details = "The polar bear is a hypercarnivorous bear whose native range lies largely within the Arctic Circle, encompassing the Arctic Ocean, its surrounding seas and surrounding land masses.",
                ImageUrl = "polarbear.jpg"
            }
        };

        Elephants = new ObservableCollection<Animal>
        {
            new Animal
            {
                Name = "African Bush Elephant",
                Location = "Africa",
                Details = "The African bush elephant, also known as the African savanna elephant, is the larger of the two species of African elephants, and the largest living terrestrial animal.",
                ImageUrl = "africanelephant.jpg"
            },
            new Animal
            {
                Name = "Indian Elephant",
                Location = "Asia",
                Details = "The Indian elephant is one of three extant recognized subspecies of the Asian elephant and native to mainland Asia.",
                ImageUrl = "indianelephant.jpg"
            },
            new Animal
            {
                Name = "Sri Lankan Elephant",
                Location = "Asia",
                Details = "The Sri Lankan elephant is one of three recognized subspecies of the Asian elephant, and native to Sri Lanka.",
                ImageUrl = "srilankanelephant.jpg"
            },
            new Animal
            {
                Name = "Mammoth",
                Location = "Extinct",
                Details = "A mammoth is any species of the extinct genus Mammuthus, one of the many genera that make up the order of trunked mammals called proboscideans.",
                ImageUrl = "mammoth.jpg"
            }
        };

        AllAnimals = new List<Animal>(Monkeys.Concat(Cats).Concat(Dogs).Concat(Bears).Concat(Elephants));
    }
}
