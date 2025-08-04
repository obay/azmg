# This is a template Homebrew formula for azmg
# To be published to a tap repository like homebrew-obay
class Azmg < Formula
  desc "Azure Management Groups and Subscriptions hierarchy visualization tool"
  homepage "https://github.com/obay/azmg"
  version "1.0.0"
  license "MIT"

  on_macos do
    if Hardware::CPU.intel?
      url "https://github.com/obay/azmg/releases/download/v1.0.0/azmg-osx-x64.tar.gz"
      sha256 ""
    end

    if Hardware::CPU.arm?
      url "https://github.com/obay/azmg/releases/download/v1.0.0/azmg-osx-arm64.tar.gz"
      sha256 ""
    end
  end

  on_linux do
    url "https://github.com/obay/azmg/releases/download/v1.0.0/azmg-linux-x64.tar.gz"
    sha256 ""
  end

  depends_on :macos => :mojave if OS.mac?

  def install
    bin.install "azmg"
  end

  test do
    assert_match "Azure Management Groups", shell_output("#{bin}/azmg --help")
  end

  def caveats
    <<~EOS
      azmg requires Azure authentication. You can use:
      - Azure CLI: 'az login'
      - Service Principal: See https://github.com/obay/azmg#authentication-options
      - Managed Identity: For Azure VMs/Services
      - Interactive: Browser-based authentication
    EOS
  end
end