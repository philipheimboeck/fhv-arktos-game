package at.fhv.tnp.arktos.controller;

import java.util.ArrayList;
import java.util.Collection;

/**
 * Author: Philip Heimböck
 * Date: 15.10.15.
 */
public abstract class InputDevice {

    private Collection<InputDeviceListener> _listener = new ArrayList<InputDeviceListener>();

    public void addListener(InputDeviceListener listener) {
        _listener.add(listener);
    }

    public void removeListener(InputDeviceListener listener) {
        _listener.remove(listener);
    }

    public void notifyListener(int code) {
        for (InputDeviceListener listener : _listener) {
            listener.onKey(code);
        }
    }

    interface InputDeviceListener {
        public void onKey(int code);
    }
}
