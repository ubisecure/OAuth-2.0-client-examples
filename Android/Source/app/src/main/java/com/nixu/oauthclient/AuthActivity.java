package com.nixu.oauthclient;

import android.net.Uri;
import android.net.http.SslError;
import android.os.AsyncTask;
import android.os.Bundle;
import android.support.v7.app.ActionBarActivity;
import android.util.Base64;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.webkit.CookieManager;
import android.webkit.SslErrorHandler;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.Button;
import android.widget.TextView;

import com.google.api.client.auth.oauth2.AuthorizationCodeRequestUrl;
import com.google.api.client.auth.oauth2.AuthorizationCodeTokenRequest;
import com.google.api.client.auth.oauth2.TokenResponse;
import com.google.api.client.http.BasicAuthentication;
import com.google.api.client.http.GenericUrl;
import com.google.api.client.http.javanet.NetHttpTransport;
import com.google.api.client.json.gson.GsonFactory;

import org.json.JSONObject;

import java.io.BufferedInputStream;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.URI;
import java.net.URL;
import java.security.cert.CertificateException;
import java.security.cert.X509Certificate;
import java.util.Arrays;

import javax.net.ssl.HttpsURLConnection;
import javax.net.ssl.SSLContext;
import javax.net.ssl.TrustManager;
import javax.net.ssl.X509TrustManager;



class Config {
    public static String authorizationEndPoint = "https://sso73.ubisecurecloudtest.com/uas/oauth2/authorization";
    public static String tokenRequestEndPoint = "https://sso73.ubisecurecloudtest.com/uas/oauth2/token";
    public static String tokenInfoEndPoint = "https://sso73.ubisecurecloudtest.com/uas/oauth2/tokeninfo";
    public static String userInfoEndPoint = "https://sso73.ubisecurecloudtest.com/uas/oauth2/userinfo";

    //Use token info instead of user info for fetching user information
    public static boolean useTokenInfo = true;

    public static String clientId = "mobile1";
    public static String clientSecret = "mobile1.secret";
    public static String redirectURI = "https://mobile1.ubidemo.com";

    public static String userInfoScope = "userinfo";
    public static String resourceSrvId = "resource1";
    public static String resourceSrvSecret = "resource1.secret";

}


public class AuthActivity extends ActionBarActivity {
    private static JSONObject userInfo = null;

    private static void trustAllHosts() {
        TrustManager[] trustAllCerts = new TrustManager[] { new X509TrustManager() {
            public java.security.cert.X509Certificate[] getAcceptedIssuers() {
                return new java.security.cert.X509Certificate[] {};
            }

            public void checkClientTrusted(X509Certificate[] chain,
                                           String authType) throws CertificateException {
            }

            public void checkServerTrusted(X509Certificate[] chain,
                                           String authType) throws CertificateException {
            }
        } };

        try {
            SSLContext sc = SSLContext.getInstance("TLS");
            sc.init(null, trustAllCerts, new java.security.SecureRandom());
            HttpsURLConnection
                    .setDefaultSSLSocketFactory(sc.getSocketFactory());
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    private void showUserInfo() {
        try {
            TextView name = (TextView) findViewById(R.id.realName);
            String usn = userInfo.getString("usn");
            for (String keyValue : usn.split(",")) {
                String[] pair = keyValue.split("=");
                if (pair.length == 2 && pair[0].equals("CN")) {
                    name.setText(pair[1]);
                    break;
                }
            }
            TextView email = (TextView) findViewById(R.id.email);
            email.setText(userInfo.getString("mail"));
            TextView phone = (TextView) findViewById(R.id.phone);
            phone.setText(userInfo.getString("mobile"));
            TextView amr = (TextView) findViewById(R.id.amr);
            amr.setText(userInfo.getString("amr").replace("\\","").replace("[", "").replace("]", "").replace("\"", ""));

            findViewById(R.id.helloView).setVisibility(View.VISIBLE);
            findViewById(R.id.webView).setVisibility(View.INVISIBLE);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    class UserInfoTask extends AsyncTask<String, Void, JSONObject> {

        private JSONObject getTokenInfo(String token) {
            try {
                HttpsURLConnection request = (HttpsURLConnection) new URL(Config.tokenInfoEndPoint).openConnection();
                request.setDoOutput(true);
                String encodedAuth = Base64.encodeToString((Config.resourceSrvId + ":" + Config.resourceSrvSecret).getBytes(), Base64.DEFAULT);
                request.setRequestProperty("Authorization", "Basic " + encodedAuth);
                request.setRequestProperty("Content-Type", "application/x-www-form-urlencoded");
                String postData = "token=" + token;
                OutputStream output = request.getOutputStream();
                output.write(postData.getBytes());
                output.close();

                byte[] data = new byte[4096];
                InputStream in = request.getInputStream();
                InputStream bin = new BufferedInputStream(in);
                bin.read(data);
                return new JSONObject(new String(data));
            } catch (Exception e) {
                return null;
            }
        }

        private JSONObject getUserInfo(String token) {
            try {
                HttpsURLConnection request = (HttpsURLConnection) new URL(Config.userInfoEndPoint).openConnection();
                request.setRequestProperty("Authorization", "Bearer " + token);
                byte[] data = new byte[4096];
                InputStream in = request.getInputStream();
                InputStream bin = new BufferedInputStream(in);
                bin.read(data);
                return new JSONObject(new String(data));
            } catch (Exception e) {
                e.printStackTrace();
            }
            return null;
        }

        protected JSONObject doInBackground(String... params) {
            String accessCode = params[0];

            TokenResponse tokenResponse = null;
            try {
                trustAllHosts();

                 tokenResponse = new AuthorizationCodeTokenRequest(new NetHttpTransport(), new GsonFactory(),
                                                                   new GenericUrl(Config.tokenRequestEndPoint), accessCode)
                        .setGrantType("authorization_code")
                        .setRedirectUri(Config.redirectURI)
                        .setClientAuthentication(new BasicAuthentication(Config.clientId, Config.clientSecret))
                        .set("client_id", Config.clientId)
                        .set("client_secret", Config.clientSecret)
                        .execute();
                String accessToken = tokenResponse.getAccessToken();
                if (Config.useTokenInfo) {
                     return getTokenInfo(accessToken);
                } else {
                    return getUserInfo(accessToken);
                }
            } catch (Exception e) {
                e.printStackTrace();
            }
            return null;
        }

        protected void onPostExecute(JSONObject tokenInfo) {
            userInfo = tokenInfo;
            showUserInfo();
        }
    }


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_auth);

        Button logout = (Button)findViewById(R.id.logout);
        logout.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                reauthorize();
            }
        });

        WebView webview = (WebView)findViewById(R.id.webView);
        webview.getSettings().setJavaScriptEnabled(true);
        webview.setWebViewClient(new WebViewClient() {
            @Override
            public void onReceivedSslError(WebView view, SslErrorHandler handler,
                                           SslError error) {
                handler.proceed();
            }

            public boolean shouldOverrideUrlLoading(WebView view, String url) {
                String accessCode = Uri.parse(url).getQueryParameter("code");

                if (accessCode != null) {
                    new UserInfoTask().execute(accessCode);
                    return true;
                } else {
                    return false;
                }

            }
        });
        if (userInfo == null) {
            reauthorize();
        } else {
            showUserInfo();
        }
    }

    private void reauthorize() {
        userInfo = null;
        CookieManager.getInstance().removeAllCookie();

        String scope = Config.useTokenInfo ? Config.resourceSrvId : Config.userInfoScope;
        URI authUrl = new AuthorizationCodeRequestUrl(Config.authorizationEndPoint, Config.clientId)
            .setRedirectUri(Config.redirectURI)
            .setScopes(Arrays.asList(scope))
            .setResponseTypes(Arrays.asList("code")).toURI();
        findViewById(R.id.helloView).setVisibility(View.INVISIBLE);
        WebView webview = (WebView)findViewById(R.id.webView);
        webview.setVisibility(View.VISIBLE);
        webview.loadUrl(authUrl.toString());
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_auth, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();

        //noinspection SimplifiableIfStatement
        if (id == R.id.action_settings) {
            return true;
        }

        return super.onOptionsItemSelected(item);
    }
}
