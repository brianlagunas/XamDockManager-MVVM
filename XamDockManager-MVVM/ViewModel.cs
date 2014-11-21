using System;
using System.Collections.ObjectModel;

namespace XamDockManager_MVVM
{
    public class ViewModel
    {
        public ObservableCollection<object> People { get; set; }

        public DelegateCommand InsertCommand { get; set; }
        public DelegateCommand RemoveCommand { get; set; }

        public ViewModel()
        {
            People = new ObservableCollection<object>();
            People.Add(new Person() { FirstName = "Brian", LastName = "Lagunas", Age = 66 });

            InsertCommand = new DelegateCommand(Insert);
            RemoveCommand = new DelegateCommand(Remove);
        }

        public void Insert(object param)
        {
            People.Add(new Person() { FirstName = String.Format("First {0}", DateTime.Now.Second), LastName = String.Format("Last {0}", DateTime.Now.Second), Age = DateTime.Now.Millisecond });
        }

        public void Remove(object param)
        {
            var person = param as Person;
            if (person != null)
                People.Remove(person);
        }
    }
}
