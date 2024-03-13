import obspython as obs
import time, sys, os, math
import numpy as np
import os.path

# Export
enabled = True
file_name = 'obs_cursor_recorder' # Write your folder in OBS Script window
folder_path = 'C:/Users/iml/Documents' # Write your folder in OBS Script window


# Global State
is_being_recorded = False
start_time_record = None
log = np.array([])
my_setting = None

count = 0 #debugging

###############################################################################
# UTILS
###############################################################################


def save_to_file():
	global log, folder_path, file_name
	global my_setting

	folder_path = obs.obs_data_get_string(my_setting, "path")
	file_name = obs.obs_data_get_string(my_setting, "name")

	full_path = os.path.join(folder_path, file_name+".csv")
	if os.path.isfile(full_path):
		full_path = os.path.join(folder_path, file_name + "_" + str(time.time()).split('.')[0] +".csv")
	
	with open(full_path, "w") as f:
		log = log.reshape((-1,3))
		np.savetxt(f, log, delimiter=',', fmt=["%i", "%f.4", "%f.4"], comments='')
		print("Saved successfully at {}".format(full_path))

	output = obs.obs_frontend_get_recording_output()
	output_settings = obs.obs_output_get_settings(output)
	print(obs.obs_data_get_string(output_settings, "path"))
	print("Saved successfully.")


###############################################################################
# HOOK HANDLER
###############################################################################

def recording_start_handler(_):
	global is_being_recorded, start_time_record, log, count
	global folder_path, file_name

	if enabled:
		is_being_recorded = True
		start_time_record = time.time()
		log = np.array([])

		print("Start Recording at {start_time}".format(start_time=start_time_record))

def recording_stopped_handler(_):
	global is_being_recorded
	is_being_recorded = False

	if enabled:
		save_to_file()
		print("Stopped Recording")
		

###############################################################################
# OBS API
###############################################################################

def script_update(settings):
	global file_name, folder_path, enabled, trigger
	global my_setting
	enabled = obs.obs_data_get_bool(settings, "enabled")
	trigger = obs.obs_data_get_string(settings, "trigger")
	
	if(is_being_recorded):
		# Don't change `enabled`
		obs.obs_data_set_bool(settings, "enabled", enabled)
	my_setting = settings


def script_tick(seconds):
	global is_being_recorded, start_time_record, log, count
	if (is_being_recorded == False) or (enabled == False):
		return

	t = time.time() #- start_time_record
	log = np.concatenate((log, np.array([count, t, 1/seconds])), axis=0)

	count += 1


def script_description():
	return '''
		<b>OBS Cursor Logger</b>
		<hr>
		Record Cursor Position on each frame, and
		<br/>
		Save the log as a csv format which an user defined.
		'''


def script_properties():
    	
	props = obs.obs_properties_create()

	enabled = obs.obs_properties_add_bool(props, "enabled", "Enabled")
	obs.obs_property_set_long_description(enabled, "Whether to save the file when recording or not.")

	path = obs.obs_properties_add_path(props, "path", "Path", obs.OBS_PATH_DIRECTORY, '*.*', None)

	name = obs.obs_properties_add_text(props, "name", "File Name", obs.OBS_TEXT_DEFAULT)
	obs.obs_property_set_long_description(name, "This name will be log's and video's")

	return props


def script_save(settings):
	script_update(settings)


def script_defaults(settings):
    	
	output = obs.obs_frontend_get_recording_output()

	signal_handler = obs.obs_output_get_signal_handler(output)
	obs.signal_handler_connect(signal_handler, 'start', recording_start_handler)
	obs.signal_handler_connect(signal_handler, 'stop', recording_stopped_handler)

	obs.obs_data_set_default_bool(settings, "enabled", True)
	obs.obs_data_set_default_string(settings, "name", "Set Your Name")
	obs.obs_data_set_default_string(settings, "path", "Set Your Path")
