import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Separator } from "@/components/ui/separator";
import { CreditCard, Building2, Wallet, CheckCircle2, ArrowLeft } from "lucide-react";
import { toast } from "sonner";
import { useNavigate } from "react-router-dom";

const Checkout = () => {
  const navigate = useNavigate();
  const [paymentMethod, setPaymentMethod] = useState("momo");
  const [isProcessing, setIsProcessing] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setIsProcessing(true);

    // Simulate payment processing
    setTimeout(() => {
      setIsProcessing(false);
      toast.success("Thanh toán thành công! Tài khoản Premium đã được kích hoạt.");
      navigate("/");
    }, 2000);
  };

  return (
    <div className="min-h-screen bg-gradient-soft">
      <main className="container mx-auto px-4 py-12">
        <div className="max-w-5xl mx-auto">
          <Button
            variant="ghost"
            onClick={() => navigate("/pricing")}
            className="mb-6"
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Quay lại
          </Button>

          <div className="grid lg:grid-cols-3 gap-8">
            {/* Payment Form */}
            <div className="lg:col-span-2">
              <Card>
                <CardHeader>
                  <CardTitle className="text-2xl">Thông tin thanh toán</CardTitle>
                  <CardDescription>
                    Hoàn tất thanh toán để kích hoạt tài khoản Premium
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <form onSubmit={handleSubmit} className="space-y-6">
                    {/* Customer Information */}
                    <div className="space-y-4">
                      <h3 className="font-semibold text-lg">Thông tin khách hàng</h3>
                      <div className="grid md:grid-cols-2 gap-4">
                        <div className="space-y-2">
                          <Label htmlFor="fullName">Họ và tên *</Label>
                          <Input
                            id="fullName"
                            placeholder="Nguyễn Văn A"
                            required
                          />
                        </div>
                        <div className="space-y-2">
                          <Label htmlFor="email">Email *</Label>
                          <Input
                            id="email"
                            type="email"
                            placeholder="example@email.com"
                            required
                          />
                        </div>
                      </div>
                      <div className="space-y-2">
                        <Label htmlFor="phone">Số điện thoại *</Label>
                        <Input
                          id="phone"
                          type="tel"
                          placeholder="0912345678"
                          required
                        />
                      </div>
                    </div>

                    <Separator />

                    {/* Payment Method */}
                    <div className="space-y-4">
                      <h3 className="font-semibold text-lg">Phương thức thanh toán</h3>
                      <RadioGroup value={paymentMethod} onValueChange={setPaymentMethod}>
                        <div className="space-y-3">
                          <label
                            htmlFor="momo"
                            className={`flex items-center gap-4 p-4 rounded-lg border-2 cursor-pointer transition-all ${
                              paymentMethod === "momo"
                                ? "border-primary bg-accent"
                                : "border-border hover:border-primary/50"
                            }`}
                          >
                            <RadioGroupItem value="momo" id="momo" />
                            <Wallet className="w-6 h-6 text-primary" />
                            <div className="flex-1">
                              <div className="font-semibold">Ví MoMo</div>
                              <div className="text-sm text-muted-foreground">
                                Thanh toán qua ví điện tử MoMo
                              </div>
                            </div>
                          </label>

                          <label
                            htmlFor="zalopay"
                            className={`flex items-center gap-4 p-4 rounded-lg border-2 cursor-pointer transition-all ${
                              paymentMethod === "zalopay"
                                ? "border-primary bg-accent"
                                : "border-border hover:border-primary/50"
                            }`}
                          >
                            <RadioGroupItem value="zalopay" id="zalopay" />
                            <Wallet className="w-6 h-6 text-primary" />
                            <div className="flex-1">
                              <div className="font-semibold">ZaloPay</div>
                              <div className="text-sm text-muted-foreground">
                                Thanh toán qua ví điện tử ZaloPay
                              </div>
                            </div>
                          </label>

                          <label
                            htmlFor="bank"
                            className={`flex items-center gap-4 p-4 rounded-lg border-2 cursor-pointer transition-all ${
                              paymentMethod === "bank"
                                ? "border-primary bg-accent"
                                : "border-border hover:border-primary/50"
                            }`}
                          >
                            <RadioGroupItem value="bank" id="bank" />
                            <Building2 className="w-6 h-6 text-primary" />
                            <div className="flex-1">
                              <div className="font-semibold">Chuyển khoản ngân hàng</div>
                              <div className="text-sm text-muted-foreground">
                                Chuyển khoản qua ATM/Internet Banking
                              </div>
                            </div>
                          </label>

                          <label
                            htmlFor="card"
                            className={`flex items-center gap-4 p-4 rounded-lg border-2 cursor-pointer transition-all ${
                              paymentMethod === "card"
                                ? "border-primary bg-accent"
                                : "border-border hover:border-primary/50"
                            }`}
                          >
                            <RadioGroupItem value="card" id="card" />
                            <CreditCard className="w-6 h-6 text-primary" />
                            <div className="flex-1">
                              <div className="font-semibold">Thẻ ATM/Visa/MasterCard</div>
                              <div className="text-sm text-muted-foreground">
                                Thanh toán bằng thẻ ngân hàng
                              </div>
                            </div>
                          </label>
                        </div>
                      </RadioGroup>
                    </div>

                    <Button
                      type="submit"
                      size="lg"
                      className="w-full"
                      disabled={isProcessing}
                    >
                      {isProcessing ? "Đang xử lý..." : "Thanh toán ngay"}
                    </Button>
                  </form>
                </CardContent>
              </Card>
            </div>

            {/* Order Summary */}
            <div className="lg:col-span-1">
              <Card className="sticky top-24">
                <CardHeader>
                  <CardTitle>Tóm tắt đơn hàng</CardTitle>
                </CardHeader>
                <CardContent className="space-y-6">
                  <div className="space-y-4">
                    <div className="flex items-start gap-3 p-4 rounded-lg bg-accent/50">
                      <div className="w-12 h-12 rounded-lg bg-gradient-primary flex items-center justify-center shadow-soft shrink-0">
                        <CheckCircle2 className="w-6 h-6 text-primary-foreground" />
                      </div>
                      <div>
                        <h3 className="font-semibold text-lg">Gói Premium</h3>
                        <p className="text-sm text-muted-foreground">Thanh toán hàng tháng</p>
                      </div>
                    </div>

                    <div className="space-y-2">
                      <div className="flex justify-between text-sm">
                        <span className="text-muted-foreground">Giá gói</span>
                        <span className="font-medium">199.000đ</span>
                      </div>
                      <div className="flex justify-between text-sm">
                        <span className="text-muted-foreground">Thuế VAT (10%)</span>
                        <span className="font-medium">19.900đ</span>
                      </div>
                    </div>

                    <Separator />

                    <div className="flex justify-between text-lg font-bold">
                      <span>Tổng cộng</span>
                      <span className="text-primary">218.900đ</span>
                    </div>
                  </div>

                  <div className="space-y-3 text-sm text-muted-foreground">
                    <div className="flex items-start gap-2">
                      <CheckCircle2 className="w-4 h-4 text-primary shrink-0 mt-0.5" />
                      <span>Tự động gia hạn hàng tháng</span>
                    </div>
                    <div className="flex items-start gap-2">
                      <CheckCircle2 className="w-4 h-4 text-primary shrink-0 mt-0.5" />
                      <span>Hủy bất cứ lúc nào</span>
                    </div>
                    <div className="flex items-start gap-2">
                      <CheckCircle2 className="w-4 h-4 text-primary shrink-0 mt-0.5" />
                      <span>Hoàn tiền 100% trong 7 ngày</span>
                    </div>
                  </div>

                  <div className="p-4 rounded-lg bg-primary/10 border border-primary/20">
                    <p className="text-sm text-foreground">
                      <strong>Ưu đãi đặc biệt:</strong> Tặng thêm 1 tháng khi đăng ký năm đầu tiên!
                    </p>
                  </div>
                </CardContent>
              </Card>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
};

export default Checkout;
