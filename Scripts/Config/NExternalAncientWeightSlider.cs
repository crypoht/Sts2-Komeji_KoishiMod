using Godot;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace KomeijiKoishi.Config;

public partial class NExternalAncientWeightSlider : Control
{
    private readonly string _key;
    private readonly Action<string, int> _onChanged;
    private int _pendingWeight;
    private bool _isReady;
    private NSlider _slider = null!;
    private Label _label = null!;

    public NExternalAncientWeightSlider(string key, int initialWeight, Action<string, int> onChanged)
    {
        _key = key;
        _onChanged = onChanged;
        _pendingWeight = int.Clamp(initialWeight, 0, 10);
        CustomMinimumSize = new Vector2(324, 64);
        Size = CustomMinimumSize;
        SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
        SizeFlagsVertical = SizeFlags.Fill;
        FocusMode = FocusModeEnum.All;

        this.TransferAllNodes(SceneHelper.GetScenePath("screens/settings_slider"));
        _slider = GetNode<NSlider>("Slider");
        _label = GetNode<Label>("SliderValue");
        _slider.MinValue = 0;
        _slider.MaxValue = 10;
        _slider.Step = 1;
        _label.Text = _pendingWeight.ToString();
    }

    public override void _Ready()
    {
        _isReady = true;
        SetWeight(_pendingWeight);
        _slider.Connect(Godot.Range.SignalName.ValueChanged, Callable.From<double>(OnValueChanged));
    }

    public void SetWeight(int weight)
    {
        int clamped = int.Clamp(weight, 0, 10);
        _pendingWeight = clamped;
        _label.Text = clamped.ToString();
        if (!_isReady)
        {
            return;
        }

        _slider.SetValueWithoutAnimation(clamped);
    }

    private void OnValueChanged(double value)
    {
        int weight = int.Clamp((int)Math.Round(value), 0, 10);
        _label.Text = weight.ToString();
        _onChanged(_key, weight);
    }
}
