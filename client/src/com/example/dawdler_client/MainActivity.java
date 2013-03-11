package com.example.dawdler_client;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.Socket;
import java.net.UnknownHostException;

import android.app.Activity;
import android.content.pm.ActivityInfo;
import android.os.Bundle;
import android.util.Log;
import android.view.Menu;
import android.view.MotionEvent;
import android.view.WindowManager;

public class MainActivity extends Activity {
	float down_x, up_x, down_y, up_y, move_x, move_y;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		getWindow().setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN,
				WindowManager.LayoutParams.FLAG_FULLSCREEN);// 设置成全屏模式

		setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_LANDSCAPE);// 强制为横屏
		start_client();
		setContentView(R.layout.activity_main);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.activity_main, menu);

		return true;
	}

	@Override
	public boolean onTouchEvent(android.view.MotionEvent event) {
		switch (event.getAction()) {
		case MotionEvent.ACTION_DOWN:
			down_x = event.getX();
			down_y = event.getY();
			move_x = down_x;
			move_y = down_y;
			break;
		case MotionEvent.ACTION_UP:
			up_x = event.getX();
			up_y = event.getY();
			if (client != null) {
				/*
				 * try { float x = up_x - down_x; float y = up_y - down_y;
				 * String msg = String.valueOf(x) + "," + String.valueOf(y);
				 * out.write(msg); out.flush(); Log.d("Touch", msg); } catch
				 * (IOException e) {
				 * 
				 * e.printStackTrace(); }
				 */
			}
			break;
		case MotionEvent.ACTION_MOVE:

			int x = (int) Math.floor(event.getX() - move_x);
			int y = (int) Math.floor(event.getY() - move_y);
			move_x = event.getX();
			move_y = event.getY();
			String msg = String.valueOf(x) + "," + String.valueOf(y);
			byte[] msg_c = msg.getBytes();
			Log.d("move", msg);
			try {
				out.write(intToBytes2(msg.length()));
				out.write(msg_c);
				out.flush();
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			break;
		}
		return true;

	};

	public static byte[] intToBytes2(int i) {
		byte[] abyte0 = new byte[4];
		abyte0[0] = (byte) (0xff & i);
		abyte0[1] = (byte) ((0xff00 & i) >> 8);
		abyte0[2] = (byte) ((0xff0000 & i) >> 16);
		abyte0[3] = (byte) ((0xff000000 & i) >> 24);
		return abyte0;
	}

	private Socket client;
	private OutputStream out;

	private void start_client() {

		Thread thread = new Thread() {
			@Override
			public void run() {
				try {
					client = new Socket("192.168.1.8", 5656);
					out = client.getOutputStream();// 获取发送数据对象
					while (true) {

						BufferedReader in = new BufferedReader(
								new InputStreamReader(client.getInputStream()));// 接收到数据对象

						// send output msg

						String inMsg = in.readLine();

						Log.i("received", inMsg);
					}

				} catch (UnknownHostException e) {
					e.printStackTrace();
					finish();
				} catch (IOException e) {
					e.printStackTrace();
					finish();
				}
			}
		};
		thread.start();

	}
}
