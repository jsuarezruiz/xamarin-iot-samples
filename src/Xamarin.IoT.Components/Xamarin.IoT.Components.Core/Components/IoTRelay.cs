﻿using System;
using System.Diagnostics;
using System.Threading;

namespace Xamarin.IoT.Components
{
	public class IoTRelay : IoTComponent, IIoTRelay
	{
		static readonly ITracer tracer = Tracer.Get<IoTRelay> ();

		const string ValueNotInRange = "Pin id parameter is not in range";
		public event EventHandler<RelayChangedEventArgs> PinChanged;
		readonly IoTPin [] pins;

		public IoTRelay (params Connectors [] gpio)
		{
			pins = new IoTPin [gpio.Length];
			for (int i = 0; i < gpio.Length; i++) {
				pins [i] = new IoTPin (gpio [i]);
				pins [i].SetDirection (IoTPinDirection.DirectionOutInitiallyLow);
				pins [i].SetActiveType (IoTActiveType.ActiveLow);
				EnablePin (i, false);
			}
		}

		public bool ContainsId (int id)
		{
			return id >= 0 && id < pins.Length;
		}

		public void Toggle (int id)
		{
			if (!ContainsId (id))
				throw new ArgumentException (ValueNotInRange);
			EnablePin (id, !GetPinValue (id));
		}

		public bool GetPinValue (int id)
		{
			if (!ContainsId (id))
				throw new ArgumentException (ValueNotInRange);
			return pins [id].Value;
		}

		public void EnablePin (int id, bool value)
		{
			if (!ContainsId (id))
				throw new ArgumentException (ValueNotInRange);
			
			var selectedPin = pins [id];
			var actualValue = GetPinValue (id);

			tracer.Verbose ($"PIN: {id} FROM '{actualValue}' TO '{value}'");

			if (actualValue == value)
				return;

			selectedPin.Value = value;
			Thread.Sleep (DefaultInstructionDelayTime);
			OnPinChanged (id, value);
		}

		void OnPinChanged (int port, bool value)
		{
			PinChanged?.Invoke(this, new RelayChangedEventArgs (port, value));
		}

		public override void Dispose ()
		{
			foreach (var pin in pins) {
				pin.Close ();
			}
		}

		public int Count()
		{
			return pins.Length;
		}
	}
}
