# Homebrew formula for azmg
# To be published to https://github.com/obay/homebrew-tap/Formula/azmg.rb
class Azmg < Formula
  desc "Azure Management Groups and Subscriptions hierarchy visualization tool"
  homepage "https://github.com/obay/azmg"
  version "1.0.0"
  license "MIT"

  on_macos do
    if Hardware::CPU.intel?
      url "https://github.com/obay/azmg/releases/download/v#{version}/azmg-osx-x64.tar.gz"
      sha256 "PLACEHOLDER_HASH_OSX_X64"
    end

    if Hardware::CPU.arm?
      url "https://github.com/obay/azmg/releases/download/v#{version}/azmg-osx-arm64.tar.gz"
      sha256 "PLACEHOLDER_HASH_OSX_ARM64"
    end
  end

  on_linux do
    if Hardware::CPU.intel?
      url "https://github.com/obay/azmg/releases/download/v#{version}/azmg-linux-x64.tar.gz"
      sha256 "PLACEHOLDER_HASH_LINUX_X64"
    end

    if Hardware::CPU.arm?
      url "https://github.com/obay/azmg/releases/download/v#{version}/azmg-linux-arm64.tar.gz"
      sha256 "PLACEHOLDER_HASH_LINUX_ARM64"
    end
  end

  def install
    bin.install "azmg"
  end

  test do
    assert_match "azmg", shell_output("#{bin}/azmg --version")
  end

  def caveats
    <<~EOS
      azmg requires Azure authentication. You can authenticate using:
      
      • Azure CLI (recommended):
        az login
      
      • Service Principal:
        Set environment variables or use configuration file
      
      • Managed Identity:
        Automatically used when running on Azure resources
      
      For more information, see:
      https://github.com/obay/azmg#authentication
    EOS
  end
end