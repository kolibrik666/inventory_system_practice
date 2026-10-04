using System;

namespace HaulSystem
{
    public sealed class HaulRoundPresenter : IDisposable
    {
        private readonly HaulRoundModel _model;
        private readonly IHaulRoundView _view;

        private bool _disposed;

        public HaulRoundPresenter(HaulRoundModel model, IHaulRoundView view)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _model.Completed += OnCompleted;

            if (_model.State == HaulRoundState.Collecting)
            {
                _view.Hide();
            }
            else
            {
                _view.Show(_model.Result);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _model.Completed -= OnCompleted;
        }

        private void OnCompleted(HaulResult result)
        {
            _view.Show(result);
        }
    }
}
