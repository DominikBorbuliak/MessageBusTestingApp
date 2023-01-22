using Utils;

namespace Services.Data
{
    /// <summary>
    /// Class that displays menu based on enum
    /// </summary>
    /// <typeparam name="E">Enum with menu items</typeparam>
    public class Menu<E> where E : struct, Enum
    {
        private const string ExitItemDisplayName = "Exit";
        private readonly string _promptBeforeMenu;
        private readonly string _promptAfterMenu;
        private readonly List<string> _menuItems;
        private int _selectedItemId;

        public Menu(string promptBeforeMenu, string promptAfterMenu, bool addExitItem = false)
        {
            _promptBeforeMenu = promptBeforeMenu;
            _promptAfterMenu = promptAfterMenu;
            _selectedItemId = 0;
            _menuItems = Enum.GetValues(typeof(E))
                            .Cast<E>()
                            .Select(value => value.GetMenuDisplayName())
                            .Where(displayName => !string.IsNullOrEmpty(displayName))
                            .ToList();

            if (addExitItem)
                _menuItems.Add(ExitItemDisplayName);
        }

        /// <summary>
        /// Displays actual state of menu
        /// </summary>
        public void DisplayMenu()
        {
            Console.Clear();

            if (!string.IsNullOrEmpty(_promptBeforeMenu))
            {
                Console.WriteLine(_promptBeforeMenu);
                Console.WriteLine();
            }

            for (var i = 0; i < _menuItems.Count; i++)
            {
                var item = $"<< {_menuItems[i]} >>";

                if (_selectedItemId == i)
                    ConsoleUtils.WriteLineColor(item, ConsoleColor.Black, ConsoleColor.White);
                else
                    Console.WriteLine(item);

            }

            if (!string.IsNullOrEmpty(_promptAfterMenu))
            {
                Console.WriteLine();
                Console.WriteLine(_promptAfterMenu);
            }
        }

        /// <summary>
        /// Handles movement in menu with arrows
        /// </summary>
        /// <returns></returns>
        public E? HandleMenuMovement()
        {
            ConsoleKey key;

            do
            {
                DisplayMenu();

                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                {
                    _selectedItemId = _selectedItemId - 1;
                    _selectedItemId += _selectedItemId < 0 ? _menuItems.Count : 0;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    _selectedItemId = (_selectedItemId + 1) % _menuItems.Count;
                }

            } while (key != ConsoleKey.Enter);

            return EnumUtils.GetValueFromMenuDisplayName<E>(_menuItems[_selectedItemId]);
        }
    }
}
