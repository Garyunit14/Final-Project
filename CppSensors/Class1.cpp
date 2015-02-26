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
	this->acc->ReadingChanged += ref new TypedEventHandler<Accelerometer ^, AccelerometerReadingChangedEventArgs ^>(this, &CppAccelerometer::accChanged);

}

void CppAccelerometer::start() {

	this->accToken = this->acc->ReadingChanged += ref new TypedEventHandler<Accelerometer ^, AccelerometerReadingChangedEventArgs ^>(this, &CppAccelerometer::accChanged);

}

void CppAccelerometer::stop() {

	this->acc->ReadingChanged -= this->accToken;

}

void CppAccelerometer::accChanged(Accelerometer ^sender, AccelerometerReadingChangedEventArgs ^args) {

	// Trigger a processing round to see if this new reading satisfies the conditions
	this->onReadingChanged(args->Reading->AccelerationX, args->Reading->AccelerationY, args->Reading->AccelerationZ);
}

