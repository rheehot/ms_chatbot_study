using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

using ChatBotApplication.Extensions;
using ChatBotApplication.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;

using Microsoft.Bot.Builder.Luis.Models;
using Newtonsoft.Json;

namespace ChatBotApplication.Dialogs
{
    [Serializable]
    public class BeautySalonRootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }



        /// <summary>
        /// Step1. 회원 여부 묻기
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync($"비긴메이트 헤어살롱 회원이시면 예를 그렇지 않으면 아니오 를  입력해주세요.");

            //2.다음단계 응답처리 함수 설정 및 수신대기
            context.Wait(HelpReplyReceivedAsync);
        }



        /// <summary>
        /// Step2. 회원여부 사용자 답변 분석하기
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task HelpReplyReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            //1.채널로부터 전달된 Activity 파라메터 수신
            var activity = await result as Activity;

            //2.대화 로직처리하기
            if (activity.Text.ToLower().Equals("yes") == true || activity.Text.ToLower().Equals("y") || activity.Text.Equals("예"))
            {
                //2.1 서비스 유형 선택 요청
                await this.ConfirmServiceTypeMessageAsync(context);
            }
            else if (activity.Text.ToLower().Equals("no") == true || activity.Text.Equals("아니오") == true )
            {
                //2.2 신규회원가입 다이얼로그 전환
                context.Call(new MembershipDialog(), ReturnRootDialogAsync);
            }
            else
            {
                //다시 이전 질문하기
                //await this.MessageReceivedAsync(context, null);

                //자연어 처리 LUIS APP호출 사용자 의도 파악하기
                await this.GetLUISIntentAsync(context, result); 
            }
        }









        /// <summary>
        /// 3.서비스 유형 선택
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task ConfirmServiceTypeMessageAsync(IDialogContext context)
        {
            var reply = context.MakeMessage();

            var options = new[]
            {
                "예약하기",
                "개인정보변경하기",
                "헤어살롱둘러보기"
            };

            reply.AddHeroCard("서비스선택", "아래 원하시는 서비스유형을 선택해주세요.", options, new[] { "http://chat.beginmate.com/Images/Eelection/hairshop1.jpg" });
            await context.PostAsync(reply);

            context.Wait(this.OnServiceTypeSelected);
        }


        /// <summary>
        /// MembershipDialog에서 루트 다이얼로그로 돌아옴
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task ReturnRootDialogAsync(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync($"신규 회원 가입 완료 후 서비스 메뉴로 이동합니다.");

            //서비스 메뉴 이동
            await this.ConfirmServiceTypeMessageAsync(context);
        }


        //예약정보 멤버변수 선언
        private ReservationModel memberReservation = null;

        /// <summary>
        /// 3.1 서비스 유형 선택 결과 처리
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task OnServiceTypeSelected(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Text == "예약하기")
            {
                memberReservation = new ReservationModel();
                await this.DesignerListMessageAsync(context, result);
            }
            else if (message.Text == "개인정보변경하기")
            {
                await context.PostAsync($"사용자 인증을 진행합니다.");
            }
            else if (message.Text == "헤어살롱둘러보기")
            {
                await this.WelcomeVideoMessageAsync(context, result);
            }
            else
            {
                await this.StartOverAsync(context, "죄송합니다. 요청사항을 이해하지 못했습니다.^^; ");
            }
        }









        /// <summary>
        /// 헤어디자이너 캐로셀 목록 메시지 발송
        /// </summary>
        /// <param name="context"></param>
        /// <param name="beforeActivity"></param>
        /// <returns></returns>
        private async Task DesignerListMessageAsync(IDialogContext context, IAwaitable<object> beforeActivity)
        {
            var activity = await beforeActivity as Activity;

            var carouselCards = new List<HeroCard>();
            carouselCards.Add(new HeroCard
            {
                Title = "1.김준오",
                Images = new List<CardImage> { new CardImage("http://chat.beginmate.com/Images/Eelection/Designer1.jpg", "1.김준오") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "선택하기", value: "1.김준오") }
            });

            carouselCards.Add(new HeroCard
            {
                Title = "2.박승철",
                Images = new List<CardImage> { new CardImage("http://chat.beginmate.com/Images/Eelection/Designer2.jpg", "2.박승철") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "선택하기", value: "2.박승철") }
            });

            carouselCards.Add(new HeroCard
            {
                Title = "3.권홍",
                Images = new List<CardImage> { new CardImage("http://chat.beginmate.com/Images/Eelection/Designer3.jpg", "3.권홍") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "선택하기", value: "3.권홍") }
            });

            carouselCards.Add(new HeroCard
            {
                Title = "4.이리안",
                Images = new List<CardImage> { new CardImage("http://chat.beginmate.com/Images/Eelection/Designer4.jpg", "4.이리안") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "선택하기", value: "4.이리안") }
            });

            var carousel = new PagedCarouselCards
            {
                Cards = carouselCards,
                TotalCount = 4
            };
        
            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = new List<Attachment>();

            foreach (HeroCard productCard in carousel.Cards)
            {
                reply.Attachments.Add(productCard.ToAttachment());
            }

            await context.PostAsync(reply);

            //사용자의 디자이너 선택정보 처리
            context.Wait(this.OnDesignerItemSelected);
        }



        /// <summary>
        /// 헤어디자이너 선택 처리
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task OnDesignerItemSelected(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            //사용자 선택 디자이너 예약정보 저장
            memberReservation.DesignerName = message.Text;

            if (message.Text == "1.김준오" || message.Text == "1")
            {
                await context.PostAsync($"김준호 디자이너를 선택하셨습니다.");
                await this.ReservationDateListMessageAsync(context, result);
            }
            else if (message.Text == "2.박승철" || message.Text == "2")
            {
                await context.PostAsync($"김준호 디자이너를 선택하셨습니다.");
                await this.ReservationDateListMessageAsync(context, result);
            }
            else if (message.Text == "3.권홍!" || message.Text == "3")
            {
                await context.PostAsync($"김준호 디자이너를 선택하셨습니다.");
                await this.ReservationDateListMessageAsync(context, result);
            }
            else if (message.Text == "4.이리안" || message.Text == "4")
            {
                await context.PostAsync($"김준호 디자이너를 선택하셨습니다.");
                await this.ReservationDateListMessageAsync(context, result);
            }
            else
            {
                await this.StartOverAsync(context, "죄송합니다. 요청사항을 이해하지 못했습니다.^^; ");
            }
        }


        /// <summary>
        /// 예외 메시지 처리 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private async Task StartOverAsync(IDialogContext context, string text)
        {
            var message = context.MakeMessage();
            message.Text = text;
            await this.StartOverAsync(context, message);
        }

        //예외 메시지 처리
        private async Task StartOverAsync(IDialogContext context, IMessageActivity message)
        {
            await context.PostAsync(message);
            await this.ConfirmServiceTypeMessageAsync(context);
        }







        /// <summary>
        /// 예약날자 선택하기
        /// </summary>
        /// <param name="context"></param>
        /// <param name="beforeActivity"></param>
        /// <returns></returns>
        private async Task ReservationDateListMessageAsync(IDialogContext context, IAwaitable<object> beforeActivity)
        {
            var activity = await beforeActivity as Activity;
            var reply = context.MakeMessage();

            var options = new[]
            {
                "04월 21일 토요일",
                "04월 22일 일요일",
                "04월 23일 월요일",
                "04월 24일 화요일",
                "04월 25일 수요일",
                "04월 26일 목요일",
            };

            reply.AddHeroCard(activity.Text, "아래 원하시는 에약날짜을 선택해주세요.", options
                , new[] { "http://chat.beginmate.com/Images/Eelection/hairshop2.jpg" });
            await context.PostAsync(reply);

            context.Wait(this.ReservationListMessageAsync);
        }




        /// <summary>
        /// 선택 디자이너 예약 가능 시간 알림
        /// </summary>
        /// <param name="context"></param>
        /// <param name="beforeActivity"></param>
        /// <returns></returns>
        private async Task ReservationListMessageAsync(IDialogContext context, IAwaitable<object> beforeActivity)
        {
            var activity = await beforeActivity as Activity;

            //사용자 예약일시 정보 저장
            memberReservation.Date = activity.Text;


            var reply = context.MakeMessage();

            var options = new[]
            {
                "오전 11시~12시",
                "오후 1시~2시",
                "오후 4시~5시",
                "이전으로",
            };

            reply.AddHeroCard(activity.Text, "아래 원하시는 에약시간을 선택해주세요.", options
                , new[] { "http://chat.beginmate.com/Images/Eelection/hairshop2.jpg" });
            await context.PostAsync(reply);

            context.Wait(this.OnReservationTimeSelected);
        }


        /// <summary>
        /// 예약시간 선택완료
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task OnReservationTimeSelected(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            memberReservation.Time = message.Text;

            if (message.Text == "오전 11시~12시" || message.Text == "1")
            {
                await context.PostAsync($" { memberReservation.DesignerName}디자이너로 { memberReservation.Date}에 { memberReservation.Time}에 예약하셨습니다.\n\n 성함을 입력해주세요.\n\n ");
                context.Wait(this.GetUserNameAsync);
            }
            else if (message.Text == "오후 1시~2시" || message.Text == "2")
            {
                await context.PostAsync($" { memberReservation.DesignerName}디자이너로 { memberReservation.Date}에 { memberReservation.Time}에 예약하셨습니다.\n\n 성함을 입력해주세요.\n\n ");
                context.Wait(this.GetUserNameAsync);
            }
            else if (message.Text == "오후 4시~5시" || message.Text == "3")
            {
                await context.PostAsync($" { memberReservation.DesignerName}디자이너로 { memberReservation.Date}에 { memberReservation.Time}에 예약하셨습니다.\n\n 성함을 입력해주세요.\n\n  ");
                context.Wait(this.GetUserNameAsync);
            }
            else if (message.Text == "이전으로")
            {
                await this.ReservationListMessageAsync(context, result);
            }
            else
            {
                await this.StartOverAsync(context, "죄송합니다. 요청사항을 이해하지 못했습니다.^^; ");
            }
        }


        /// <summary>
        /// 이름받기
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task GetUserNameAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            memberReservation.MemberName = activity.Text;

            await context.PostAsync($"전화번호를 입력해주세요.");
            context.Wait(this.GetUserTelephoneAsync);
        }




        /// <summary>
        /// 전화번호 받기
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task GetUserTelephoneAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result as Activity;
            memberReservation.Telephone = activity.Text;

            await context.PostAsync($"감사합니다. 예약이 완료되었습니다.\n\n ");
            await this.ConfirmServiceTypeMessageAsync(context);
        }



        /// <summary>
        /// 미용실 소개 동영상 메시지 처리
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task WelcomeVideoMessageAsync(IDialogContext context, IAwaitable<object> beforeActivity)
        {
            var reply = context.MakeMessage();

            var videoCard = new VideoCard
            {
                Title = "비긴메이트 헤어살롱",
                Subtitle = "스타트업 종사분들만 모시는 스타트업 O2O 헤어살롱입니다.\n\n  커피공짜,사무실 공짜, 언제든 머리고 복잡할때 찾아주세요.\n\n 시원하게 밀어드립니다.",
                Text = "",
                Image = new ThumbnailUrl
                {
                    Url = "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c5/Big_buck_bunny_poster_big.jpg/220px-Big_buck_bunny_poster_big.jpg"
                },
                Media = new List<MediaUrl>
                {
                    new MediaUrl()
                    {
                        Url = "http://download.blender.org/peach/bigbuckbunny_movies/BigBuckBunny_320x180.mp4"
                    }
                },
                Buttons = new List<CardAction>
                {
                    new CardAction()
                    {
                        Title = "자세히 보기",
                        Type = ActionTypes.OpenUrl,
                        Value = "http://www.beginmate.com"
                    },
                    new CardAction()
                    {
                        Title = "이전으로",
                        Type = ActionTypes.ImBack,
                        Value = "이전으로"
                    }
                }
            };

            reply.Attachments.Add(videoCard.ToAttachment());
            await context.PostAsync(reply);
            context.Wait(this.OnWelcomMsgSelected);
        }

        private async Task OnWelcomMsgSelected(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            await this.StartOverAsync(context, "감사합니다. 서비스 목록으로 이동합니다.");
        }


        /// <summary>
        /// 자연어처리(LUIS) 를 이용한 사용자 의도 파악
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task GetLUISIntentAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            string intent = "테스트";

            var client = new HttpClient();
            var uri = "https://westus.api.cognitive.microsoft.com/luis/v2.0"
                +"/apps/c5d5e819-10ae-477e-87f2-7f357f2dda82?subscription-key=eee7c9a1669449f1b1adef6c13ba5b49&verbose=true&timezoneOffset=540"
                +"&q=" + activity.Text;
            var response = await client.GetAsync(uri);

            var strResponseContent = await response.Content.ReadAsStringAsync();

            LuisResult objResult  = JsonConvert.DeserializeObject<LuisResult>(strResponseContent);
            intent = objResult.TopScoringIntent.Intent;
            await context.PostAsync($"사용자 최적화된 의도는 {objResult.TopScoringIntent.Intent} 이고 적합도는 {objResult.TopScoringIntent.Score} 입니다.");

            switch(intent)
            {
                case "예약":
                    await context.PostAsync($"바로 예약하시겠습니까?");
                    //context.Wait(TobeContinuedReservationProcess);
                    break;
                case "위치":
                    await context.PostAsync($"위치와 약도를 안내해드릴까요?");
                    //context.Wait(TobeContinuedContactUsProcess);
                    break;
            }
        }



    }
}