using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using System.Reflection;

namespace AndroidDeviceHelper.View.CustomizedControls
{
    class TextEditorWrapper
    {
        private static readonly Type TextEditorType = Type.GetType("System.Windows.Documents.TextEditor, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
        private static readonly PropertyInfo IsReadOnlyProp = TextEditorType.GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly PropertyInfo TextViewProp = TextEditorType.GetProperty("TextView", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly PropertyInfo SelectionProp = TextEditorType.GetProperty("Selection", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo RegisterMethod = TextEditorType.GetMethod("RegisterCommandHandlers",
            BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Type), typeof(bool), typeof(bool), typeof(bool) }, null);

        private static readonly Type TextContainerType = Type.GetType("System.Windows.Documents.ITextContainer, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
        private static readonly PropertyInfo TextContainerTextViewProp = TextContainerType.GetProperty("TextView");
        //private static readonly PropertyInfo TextContainerSymbolCountProp = TextContainerType.GetProperty("SymbolCount");
        private static readonly PropertyInfo TextContainerProp = typeof(TextBlock).GetProperty("TextContainer", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void RegisterCommandHandlers(Type controlType, bool acceptsRichContent, bool readOnly, bool registerEventListeners)
        {
            RegisterMethod.Invoke(null, new object[] { controlType, acceptsRichContent, readOnly, registerEventListeners });
        }

        public static TextEditorWrapper CreateFor(TextBlock tb)
        {
            var textContainer = TextContainerProp.GetValue(tb);

            var editor = new TextEditorWrapper(textContainer, tb, false);
            IsReadOnlyProp.SetValue(editor._editor, true);
            TextViewProp.SetValue(editor._editor, TextContainerTextViewProp.GetValue(textContainer));

            return editor;
        }

        private readonly object _editor;
        private readonly TextSelection selection;
        public readonly int TextContainer_SymbolCount;
        public TextEditorWrapper(object textContainer, FrameworkElement uiScope, bool isUndoEnabled)
        {
            _editor = Activator.CreateInstance(TextEditorType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance,
                null, new[] { textContainer, uiScope, isUndoEnabled }, null);
            selection = (TextSelection)SelectionProp.GetValue(_editor);
            //TextContainer_SymbolCount = int.Parse(TextContainerSymbolCountProp.GetValue(textContainer).ToString());
        }
        public void Select(TextPointer start, TextPointer end)
        {
            selection.Select(start, end);
        }
    }
    public class SelectableTextBlock : TextBlock
    {
        static SelectableTextBlock()
        {
            FocusableProperty.OverrideMetadata(typeof(SelectableTextBlock), new FrameworkPropertyMetadata(true));
            TextEditorWrapper.RegisterCommandHandlers(typeof(SelectableTextBlock), true, true, true);

            // remove the focus rectangle around the control
            FocusVisualStyleProperty.OverrideMetadata(typeof(SelectableTextBlock), new FrameworkPropertyMetadata((object)null));
        }

        private readonly TextEditorWrapper _editor;

        public SelectableTextBlock()
        {
            _editor = TextEditorWrapper.CreateFor(this);
            PreviewMouseLeftButtonUp += SelectableTextBlock_MouseDown;
        }

        private void SelectableTextBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Select(1, 9);
        }

        public void SelectAll()
        {
            _editor.Select(ContentStart, ContentEnd);
        }
        public void Select(int start, int length)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException("start", "parameter start mustn't be less then zero");
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", "parameter length mustn't be less then zero");
            }
            start += 1;
            //int maxStart = _editor.TextContainer_SymbolCount;
            //if (start > maxStart)
            //{
            //    start = maxStart;
            //}
            TextPointer newStart = ContentStart.GetPositionAtOffset(start);
            // Identify new position for selection End
            int maxLength = newStart.GetOffsetToPosition(ContentEnd);
            if (length > maxLength)
            {
                length = maxLength;
            }
            TextPointer newEnd = newStart.GetPositionAtOffset(length);

            // Normalize end in some particular direction to exclude ambiguity on surrogate boundaries
            newEnd = newEnd.GetInsertionPosition(LogicalDirection.Forward);
            _editor.Select(newStart, newEnd);
        }
    }
}
