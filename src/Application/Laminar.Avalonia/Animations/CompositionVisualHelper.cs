using System.Runtime.CompilerServices;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;

namespace Laminar.Avalonia.Animations;

public static class CompositionVisualHelper
{
    private const string Offset = nameof(CompositionVisual.Offset);
    private const string Translation = nameof(CompositionVisual.Translation);
    private const string Scale = nameof(CompositionVisual.Scale);
    
    extension(CompositionVisual visual)
    {
        public void SetImplicitOffsetAnimation(Vector3KeyFrameAnimation? animation)
        {
            if (animation is null)
            {
                ImplicitAnimationsFor(visual).Remove(Offset);
            }
            else
            {
                ImplicitAnimationsFor(visual)[Offset] = animation;
            }
        }

        public void SetImplicitTranslationAnimation(Vector3KeyFrameAnimation? animation)
        {
            if (animation is null)
            {
                ImplicitAnimationsFor(visual).Remove(Translation);
            }
            else
            {
                ImplicitAnimationsFor(visual)[Translation] = animation;
            }
        }

        public void SetImplicitScaleAnimation(Vector3KeyFrameAnimation? animation)
        {
            if (animation is null)
            {
                ImplicitAnimationsFor(visual).Remove(Scale);
            }
            else
            {
                ImplicitAnimationsFor(visual)[Scale] = animation;
            }
        }
    }

    private static ImplicitAnimationCollection ImplicitAnimationsFor(CompositionVisual visual)
        => visual.ImplicitAnimations ?? (visual.ImplicitAnimations = visual.Compositor.CreateImplicitAnimationCollection());
}