#pragma once
using namespace Windows::Devices::Sensors;
using namespace Windows::Foundation;

namespace CppSensors
{
	public delegate void ProcessingFinished(bool matched);

	public ref class CppAccelerometer sealed
	{


	public:
		CppAccelerometer();
		void accChanged(Accelerometer ^sender, AccelerometerReadingChangedEventArgs ^args);
		void start();
		void stop();
		event ProcessingFinished^ onProcessingFinished;

	private:
		Accelerometer^ acc;
		EventRegistrationToken accToken;

		float accX, accY, accZ;
		void process();
	};
}