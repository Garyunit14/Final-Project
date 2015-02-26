// Class1.cpp
#include "pch.h"
#include "Class1.h"
#include <math.h>

using namespace CppSensors;
using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Devices::Sensors;

CppAccelerometer::CppAccelerometer()
{
	this->acc = Accelerometer::GetDefault();
	this->acc->ReportInterval = this->acc->MinimumReportInterval;
	this->accX = this->accY = this->accZ = 0.0f;

}

void CppAccelerometer::start() {

	this->accToken = this->acc->ReadingChanged += ref new TypedEventHandler<Accelerometer ^, AccelerometerReadingChangedEventArgs ^>(this, &CppAccelerometer::accChanged);

}

void CppAccelerometer::stop() {

	this->acc->ReadingChanged -= this->accToken;

}

void CppAccelerometer::accChanged(Accelerometer ^sender, AccelerometerReadingChangedEventArgs ^args) {
	// Cache the new readings into our class
	this->accX = (float)args->Reading->AccelerationX;
	this->accY = (float)args->Reading->AccelerationY;
	this->accZ = (float)args->Reading->AccelerationZ;

	// Trigger a processing round to see if this new reading satisfies the conditions
	this->process();
}

void CppAccelerometer::process() {

	// If the accelerometer vector isn't pointing straight in the Y direction, just return
	if (abs(this->accX) > 0.2f || abs(this->accY) > 0.2f || this->accZ > -0.8f) {
		this->onProcessingFinished(false);
		return;
	}

	// If we've made it this far, let's trigger the event!
	this->onProcessingFinished(true);
}
