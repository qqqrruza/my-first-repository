using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DataBaseLibrary;

namespace SportSchoolAPP
{
    public partial class Form1 : Form
    {
        private BindingSource bindingSource = new BindingSource();
        private string currentTable;
        private List<Section> sections; // Список секций для проверки SectionID
        private List<Group> groups; // Список групп для проверки GroupID
        private List<Coach> coaches; // Список тренеров для проверки CoachID

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBoxTables.Items.AddRange(new string[] { "Секции", "Тренеры", "Группы", "Дети", "Расписание" });

            // Загрузка данных секций, групп и тренеров при запуске формы
            sections = Database.LoadData<Section>("sections");
            groups = Database.LoadData<Group>("groups");
            coaches = Database.LoadData<Coach>("coaches");

            // Добавление обработчиков событий для проверки уникальности
            dataGridView.CellValidating += DataGridView_CellValidating;
            dataGridView.RowValidating += DataGridView_RowValidating;
        }

        private void comboBoxTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentTable = comboBoxTables.SelectedItem.ToString().ToLower();
            LoadTableData(currentTable);
        }

        private void LoadTableData(string tableName)
        {
            switch (tableName)
            {
                case "секции":
                    bindingSource.DataSource = Database.LoadData<Section>("sections");
                    dataGridView.DataSource = bindingSource;
                    SetColumnHeaders(new string[] { "ID", "Название", "Стоимость" });
                    break;
                case "тренеры":
                    bindingSource.DataSource = Database.LoadData<Coach>("coaches");
                    dataGridView.DataSource = bindingSource;
                    SetColumnHeaders(new string[] { "ID", "Имя", "ID секции" });
                    break;
                case "группы":
                    bindingSource.DataSource = Database.LoadData<Group>("groups");
                    dataGridView.DataSource = bindingSource;
                    SetColumnHeaders(new string[] { "ID", "ID секции", "Количество детей" });
                    break;
                case "дети":
                    bindingSource.DataSource = Database.LoadData<Children>("children");
                    dataGridView.DataSource = bindingSource;
                    SetColumnHeaders(new string[] { "ID", "Имя", "Возраст", "ID группы" });
                    break;
                case "расписание":
                    bindingSource.DataSource = Database.LoadData<Schedule>("schedules");
                    dataGridView.DataSource = bindingSource;
                    SetColumnHeaders(new string[] { "ID", "ID секции", "ID тренера", "Дата", "ID группы" });
                    break;
                default:
                    bindingSource.DataSource = null;
                    break;
            }
        }

        private void SetColumnHeaders(string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                dataGridView.Columns[i].HeaderText = headers[i];
            }
        }

        private void btnSaveChanges_Click(object sender, EventArgs e)
        {
            SaveChanges();
        }

        private void SaveChanges()
        {
            if (currentTable != null)
            {
                try
                {
                    switch (currentTable)
                    {
                        case "секции":
                            ValidateAndSave(bindingSource.List.Cast<Section>().ToList(), "sections");
                            break;
                        case "тренеры":
                            ValidateAndSave(bindingSource.List.Cast<Coach>().ToList(), "coaches");
                            break;
                        case "группы":
                            ValidateAndSave(bindingSource.List.Cast<Group>().ToList(), "groups");
                            break;
                        case "дети":
                            ValidateAndSave(bindingSource.List.Cast<Children>().ToList(), "children");
                            break;
                        case "расписание":
                            ValidateAndSave(bindingSource.List.Cast<Schedule>().ToList(), "schedules");
                            break;
                    }
                    MessageBox.Show("Изменения успешно сохранены!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}");
                }
            }
        }

        private void ValidateAndSave<T>(List<T> list, string tableName) where T : class
        {
            // Проверка уникальности записей в list
            var ids = new HashSet<int>();
            foreach (var item in list)
            {
                var idProperty = item.GetType().GetProperty("ID");
                if (idProperty != null)
                {
                    int id = (int)idProperty.GetValue(item);
                    if (!ids.Add(id))
                    {
                        throw new Exception("Обнаружены дублирующиеся записи с ID: " + id);
                    }
                }
            }

            Database.UpdateData(list, tableName);
        }

        private void DataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (currentTable == "расписание")   
            {
                if (dataGridView.Columns[e.ColumnIndex].HeaderText == "ID группы")
                {
                    if (!int.TryParse(e.FormattedValue.ToString(), out int groupID))
                    {
                        e.Cancel = true;
                        MessageBox.Show("ID группы должно быть числом.");
                        return;
                    }

                    // Проверка наличия GroupID в таблице Groups
                    if (groups != null && groups.All(g => g.ID != groupID))
                    {
                        e.Cancel = true;
                        MessageBox.Show("Введенный ID группы не существует.");
                    }
                }
                else if (dataGridView.Columns[e.ColumnIndex].HeaderText == "ID секции")
                {
                    if (!int.TryParse(e.FormattedValue.ToString(), out int sectionID))
                    {
                        e.Cancel = true;
                        MessageBox.Show("ID секции должно быть числом.");
                        return;
                    }

                    // Проверка наличия SectionID в таблице Sections
                    if (sections != null && sections.All(s => s.ID != sectionID))
                    {
                        e.Cancel = true;
                        MessageBox.Show("Введенный ID секции не существует.");
                    }
                }
                else if (dataGridView.Columns[e.ColumnIndex].HeaderText == "ID тренера")
                {
                    if (!int.TryParse(e.FormattedValue.ToString(), out int coachID))
                    {
                        e.Cancel = true;
                        MessageBox.Show("ID тренера должно быть числом.");
                        return;
                    }

                    // Проверка наличия CoachID в таблице Coaches
                    if (coaches != null && coaches.All(c => c.ID != coachID))
                    {
                        e.Cancel = true;
                        MessageBox.Show("Введенный ID тренера не существует.");
                    }
                }
            }
        }

        private void DataGridView_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            // Дополнительная проверка уникальности перед завершением редактирования строки
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Index != e.RowIndex && row.Cells["ID"].Value != null && dataGridView.Rows[e.RowIndex].Cells["ID"].Value != null &&
                    (int)row.Cells["ID"].Value == (int)dataGridView.Rows[e.RowIndex].Cells["ID"].Value)
                {
                    e.Cancel = true;
                    MessageBox.Show("ID должно быть уникальным.");
                    return;
                }
            }
        }
    }
}
