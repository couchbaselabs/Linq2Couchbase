using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Couchbase.Linq.Utils;

namespace Couchbase.Linq.Metadata
{
    /// <summary>
    /// Metadata about a type inherited from <see cref="BucketContext"/>.
    /// </summary>
    internal class ContextMetadata
    {
        private static readonly Type[] DocumentSetConstructorArgumentTypes =
        {
            typeof(BucketContext),
            typeof(string),
            typeof(string)
        };

        /// <summary>
        /// The type inherited from <see cref="BucketContext"/>.
        /// </summary>
        public Type ContextType { get; }

        /// <summary>
        /// Properties that return <see cref="IDocumentSet{T}"/>.
        /// </summary>
        public DocumentSetMetadata[] Properties { get; }

        /// <summary>
        /// An action that initializes <see cref="Properties"/> with <see cref="DocumentSet{T}"/> instances.
        /// </summary>
        private Action<BucketContext> Initialize { get; }

        public ContextMetadata(Type contextType)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (contextType == null)
            {
                ThrowHelpers.ThrowArgumentNullException(nameof(contextType));
            }

            ContextType = contextType;
            Properties = contextType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && IsDocumentSet(p.PropertyType))
                .Select(p => new DocumentSetMetadata(p))
                .ToArray();

            Initialize = CreateInitializeAction();
        }

        /// <summary>
        /// Compiles an action which will initialize the BucketContext properties. We use a compiled action
        /// instead of reflection here for speed. This executes once the first time a given type is used,
        /// after which the action is reused for each new instance of BucketContext.
        /// </summary>
#if NET5_0_OR_GREATER
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(DocumentSet<>))]
#endif
        private Action<BucketContext> CreateInitializeAction()
        {
            var dynMethod = new DynamicMethod("Initialize", null, new[] { typeof(BucketContext) })
            {
                InitLocals = false // Don't need this, very minor perf improvement
            };

            var il = dynMethod.GetILGenerator();
            il.DeclareLocal(ContextType);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, ContextType);
            il.Emit(OpCodes.Stloc_0);

            foreach (var property in Properties)
            {
                // load for property setter
                il.Emit(OpCodes.Ldloc_0);


                // load again for constructor
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldstr, property.CollectionInfo.Scope);
                il.Emit(OpCodes.Ldstr, property.CollectionInfo.Collection);

                var documentSetType = typeof(DocumentSet<>).MakeGenericType(property.DocumentType);
                var documentSetConstructor = documentSetType.GetConstructor(DocumentSetConstructorArgumentTypes)!;
                il.Emit(OpCodes.Newobj, documentSetConstructor);
                il.Emit(OpCodes.Castclass, typeof(IDocumentSet<>).MakeGenericType(property.DocumentType));

                var setter = property.Property.GetSetMethod()!;
                il.Emit(OpCodes.Callvirt, setter);
            }

            il.Emit(OpCodes.Ret);

            return (Action<BucketContext>)dynMethod.CreateDelegate(typeof(Action<BucketContext>));
        }

        public void Fill(BucketContext bucketContext)
        {
            Debug.Assert(ContextType.IsInstanceOfType(bucketContext));

            Initialize(bucketContext);
        }

        private static bool IsDocumentSet(Type propertyType) =>
            propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IDocumentSet<>);
    }
}
