using LDtkLevelManager;
using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkLevelManagerEditor
{
    public delegate void PaginationChangedEvent(PaginationInfo pagination);
    public class PaginatorElement : VisualElement
    {
        private const string TemplateName = "PaginatorInspector";


        private DropdownField _fieldItemsPerPage;
        private Label _labelPageIndex;
        private Label _labelTotalOfPages;
        private Button _buttonFirst;
        private Button _buttonPrevious;
        private Button _buttonNext;
        private Button _buttonLast;
        private Label _labelTotal;

        private PaginationInfo _pagination;
        private int _totalOfItems;

        public PaginationInfo Pagination => _pagination;
        public int TotalOfItems
        {
            get => _totalOfItems;
            set
            {
                _totalOfItems = value;
                _labelTotal.text = _totalOfItems.ToString();
                UpdateDisplay();
            }
        }

        public int LastPage => (int)Mathf.Ceil((float)_totalOfItems / _pagination.PageSize);

        public event PaginationChangedEvent PaginationChanged;

        public PaginatorElement()
        {
            TemplateContainer container = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();

            _fieldItemsPerPage = container.Q<DropdownField>("field-items-per-page");
            _labelPageIndex = container.Q<Label>("label-page-index");
            _labelTotalOfPages = container.Q<Label>("label-total-of-pages");
            _buttonFirst = container.Q<Button>("button-first");
            _buttonPrevious = container.Q<Button>("button-previous");
            _buttonNext = container.Q<Button>("button-next");
            _buttonLast = container.Q<Button>("button-last");
            _labelTotal = container.Q<Label>("label-total");

            _pagination = new()
            {
                PageIndex = 1,
                PageSize = 5,
            };

            SetupItemsPerPage();
            SetupButtons();

            Add(container);
        }

        #region Setups

        private void SetupItemsPerPage()
        {
            _fieldItemsPerPage.choices = new()
            {
                "5",
                "10",
                "20",
            };
            _fieldItemsPerPage.value = "5";
            _fieldItemsPerPage.RegisterValueChangedCallback(evt => OnPerPageChanged(evt.newValue));
        }

        private void SetupButtons()
        {
            _buttonFirst.clicked += OnFirst;
            _buttonPrevious.clicked += OnPrevious;
            _buttonNext.clicked += OnNext;
            _buttonLast.clicked += OnLast;
        }

        /// <summary>
        /// Updates the display of the paginator element.
        /// </summary>
        private void UpdateDisplay()
        {
            // Update the display of the paginator element based on the current page index and total number of items.

            // Update the value of the items per page field without notifying the change to avoid infinite recursion.
            _fieldItemsPerPage.SetValueWithoutNotify(_pagination.PageSize.ToString());

            // Update the display of the page index and total number of pages.
            _labelPageIndex.text = _pagination.PageIndex.ToString();
            _labelTotalOfPages.text = LastPage.ToString();

            // Update the enabled state of the buttons based on the current page index.
            if (_pagination.PageIndex - 1 == 0 && _pagination.PageIndex == LastPage)
            {
                // Disable all buttons if the current page index is 1 and it is the last page.
                _buttonFirst.SetEnabled(false);
                _buttonPrevious.SetEnabled(false);
                _buttonNext.SetEnabled(false);
                _buttonLast.SetEnabled(false);
            }
            else if (_pagination.PageIndex - 1 == 0)
            {
                // Disable the first and previous buttons if the current page index is 1 and it is not the last page.
                _buttonFirst.SetEnabled(false);
                _buttonPrevious.SetEnabled(false);
                _buttonNext.SetEnabled(true);
                _buttonLast.SetEnabled(true);
            }
            else if (_pagination.PageIndex + 1 > LastPage)
            {
                // Disable the next and last buttons if the current page index is greater than the last page index.
                _buttonFirst.SetEnabled(true);
                _buttonPrevious.SetEnabled(true);
                _buttonNext.SetEnabled(false);
                _buttonLast.SetEnabled(false);
            }
            else
            {
                // Enable all buttons if the current page index is neither 1 nor the last page index.
                _buttonFirst.SetEnabled(true);
                _buttonPrevious.SetEnabled(true);
                _buttonNext.SetEnabled(true);
                _buttonLast.SetEnabled(true);
            }
        }

        #endregion

        #region Callbacks

        private void OnPerPageChanged(string newValue)
        {
            _pagination.PageSize = int.Parse(newValue);
            _pagination.PageIndex = 1;
            PaginationChanged?.Invoke(_pagination);
            UpdateDisplay();
        }

        private void OnFirst()
        {
            _pagination.PageIndex = 1;
            PaginationChanged?.Invoke(_pagination);
            UpdateDisplay();
        }

        private void OnPrevious()
        {
            if (_pagination.PageIndex <= 1) return;
            _pagination.PageIndex--;
            PaginationChanged?.Invoke(_pagination);
            UpdateDisplay();
        }

        private void OnNext()
        {
            if (_pagination.PageIndex + 1 > LastPage) return;
            _pagination.PageIndex++;
            PaginationChanged?.Invoke(_pagination);
            UpdateDisplay();
        }

        private void OnLast()
        {
            _pagination.PageIndex = LastPage;
            PaginationChanged?.Invoke(_pagination);
            UpdateDisplay();
        }

        #endregion
    }
}