using System;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;
using ReSharper.Exceptional.Analyzers;

namespace ReSharper.Exceptional.Models
{
    internal class ExceptionDocCommentModel : ModelBase
    {
        public ExceptionDocCommentModel(DocCommentBlockModel documentationBlock, string exceptionType, string exceptionDescription)
            : base(documentationBlock.AnalyzeUnit)
        {
            DocumentationBlock = documentationBlock;

            ExceptionTypeName = exceptionType;
            ExceptionType = GetExceptionType(exceptionType);
            ExceptionDescription = exceptionDescription;
        }

        public DocCommentBlockModel DocumentationBlock { get; private set; }

        public IDeclaredType ExceptionType { get; private set; }

        public string ExceptionTypeName { get; private set; }

        public string ExceptionDescription { get; private set; }

        public override DocumentRange DocumentRange
        {
            get { return GetCommentRange(); }
        }

        public override void Accept(AnalyzerBase analyzer)
        {
            analyzer.Visit(this);
        }

        private IDeclaredType GetExceptionType(string exceptionType)
        {
            var exceptionReference = DocumentationBlock.References.Find(reference => reference.GetName().Equals(exceptionType));
            var psiModule = DocumentationBlock.Node.GetPsiModule();

            if (exceptionReference == null)
                return TypeFactory.CreateTypeByCLRName(exceptionType, psiModule, psiModule.GetContextFromModule());
            else
            {
                var resolveResult = exceptionReference.Resolve();
                var declaredType = resolveResult.DeclaredElement as ITypeElement;
                if (declaredType == null)
                    return TypeFactory.CreateTypeByCLRName(exceptionType, psiModule, psiModule.GetContextFromModule());
                else
                    return TypeFactory.CreateType(declaredType);
            }
        }

        public DocumentRange GetMarkerRange()
        {
            var text = DocumentationBlock.Node.GetText();
            if (text.Contains(Constants.ExceptionDescriptionMarker))
            {
                var documentRange = DocumentationBlock.Node.GetDocumentRange();
                var textRange = documentRange.TextRange;

                var index = text.IndexOf(Constants.ExceptionDescriptionMarker, StringComparison.InvariantCulture);
                var startOffset = textRange.StartOffset + index;
                var endOffset = startOffset + 8;

                var newTextRange = new TextRange(startOffset, endOffset);
                return new DocumentRange(documentRange.Document, newTextRange);
            }
            return DocumentRange.InvalidRange;
        }

        private DocumentRange GetCommentRange()
        {
            var text = DocumentationBlock.Node.GetText();
            var documentRange = DocumentationBlock.DocumentRange;
            var textRange = documentRange.TextRange;

            var tagStart = "<exception cref=\"";
            var xml = tagStart + ExceptionTypeName + "\"";
            var index = text.IndexOf(xml, StringComparison.InvariantCulture);

            var startOffset = textRange.StartOffset + index + tagStart.Length;
            var endOffset = startOffset + ExceptionTypeName.Length;

            var newTextRange = new TextRange(startOffset, endOffset);
            return new DocumentRange(documentRange.Document, newTextRange);
        }
    }
}