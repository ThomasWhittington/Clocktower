import DiscordLoginButton
    from "@/components/auth/DiscordLoginButton";

const LoginPage = () => {
    const urlParams = new URLSearchParams(window.location.search);
    const error = urlParams.get('error');

    return (
        <div className="min-h-screen flex items-center justify-center bg-gray-50">
            <div className="max-w-md w-full space-y-8">
                <div>
                    <h2 className="mt-6 text-center text-3xl font-extrabold text-gray-900">
                        Sign in to your account
                    </h2>
                </div>
                <div className="mt-8 space-y-6">
                    {error && (
                        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
                            Authentication failed. Please try again.
                        </div>
                    )}
                    <div className="flex justify-center">
                        <DiscordLoginButton />
                    </div>
                </div>
            </div>
        </div>
    );
};

export default LoginPage;