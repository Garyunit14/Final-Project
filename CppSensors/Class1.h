#pragma once
using namespace Windows::Devices::Sensors;
using namespace Windows::Foundation;

namespace CppSensors
{
	public delegate void AccelermeterEvent(double x, double y, double z);

	public ref class CppAccelerometer sealed
	{


	public:
		CppAccelerometer();
		void accChanged(Accelerometer ^sender, AccelerometerReadingChangedEventArgs ^args);
		void start();
		void stop();
		event AccelermeterEvent^ onReadingChanged;

	private:
		Accelerometer^ acc;
		EventRegistrationToken accToken;

	};
}