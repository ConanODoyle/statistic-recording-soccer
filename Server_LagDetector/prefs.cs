$funcPass = 8564862;
if(isFunction("RTB_registerPref")) {
  RTB_registerPref("Delta Threshold"  , "Lag Detector", "$LagDetector::Delta"   , "int 50 1000", "Server_LagDetector", 40 , 0, 0, "");
  RTB_registerPref("Ping Threshold"   , "Lag Detector", "$LagDetector::Min"     , "int 10 1000", "Server_LagDetector", 200, 0, 0, "");
  RTB_registerPref("Refresh Rate (ms)", "Lag Detector", "$LagDetector::TickRate", "int 33 1000", "Server_LagDetector", 100, 0, 0, "");
  RTB_registerPref("Lockout Time (ms)", "Lag Detector", "$LagDetector::Lock"    , "int 33 1000", "Server_LagDetector", 300, 0, 0, "");
} else {
  $LagDetector::Delta    = 60;
  $LagDetector::Min      = 140;
  $LagDetector::TickRate = 250;
  $LagDetector::Lock     = 300;
}
