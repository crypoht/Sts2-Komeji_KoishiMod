import json, re
from pathlib import Path

root = Path(r'D:\UINya\Godot_v4.5.1-stable_mono_win64\komeiji-koishi\Komeiji_Koishi\localization')
eng_dir = root / 'eng'

# Broad term maps tuned to official wording in D:\UINya\jpn and D:\UINya\kor.
jp_terms = {
    'Unconscious': '無意識', 'Instinctive': '本能', 'Danmaku': '弾幕', 'Bloom': '開花', 'Closed': '閉合',
    'Block': 'ブロック', 'Intangible': '霊体', 'Weak': '脱力', 'Vulnerable': '弱体', 'Thorns': 'トゲ',
    'Tracing': '追跡', 'Strength': '筋力', 'Dexterity': '敏捷', 'Plated Armor': 'プレートアーマー',
    'Rolling Boulder': '転がる巨石', 'Gold': 'ゴールド', 'Rare': 'レア', 'Curse': '呪い', 'Miracle': '奇跡',
    'Moriya Dance': '守矢大舞', 'Koishi Komeiji': '古明地こいし', 'Koishi': 'こいし',
    'Yin-Yang Orb Danmaku': '陰陽玉弾幕', 'Rose Danmaku': '薔薇弾幕', 'Star Danmaku': '星弾幕', 'Heart Danmaku': 'ハート弾幕',
    'Conscious Overflow': '意識の氾濫', 'Night Sparrow\'s Wings': '夜雀の翼',
}
kr_terms = {
    'Unconscious': '무의식', 'Instinctive': '본능', 'Danmaku': '탄막', 'Bloom': '개화', 'Closed': '폐쇄',
    'Block': '방어도', 'Intangible': '불가침', 'Weak': '약화', 'Vulnerable': '취약', 'Thorns': '가시',
    'Tracing': '추적', 'Strength': '힘', 'Dexterity': '민첩', 'Plated Armor': '판금 갑옷',
    'Rolling Boulder': '구르는 바위', 'Gold': '골드', 'Rare': '희귀', 'Curse': '저주', 'Miracle': '기적',
    'Moriya Dance': '모리야 대무', 'Koishi Komeiji': '코메이지 코이시', 'Koishi': '코이시',
    'Yin-Yang Orb Danmaku': '음양옥 탄막', 'Rose Danmaku': '장미 탄막', 'Star Danmaku': '별 탄막', 'Heart Danmaku': '하트 탄막',
    'Conscious Overflow': '의식의 범람', 'Night Sparrow\'s Wings': '밤참새의 날개',
}

def protect(s):
    parts=[]
    def repl(m):
        parts.append(m.group(0)); return f'§{len(parts)-1}§'
    # protect localization tags, variables, rich text tags, condition chunks
    s = re.sub(r'\{[^{}]*\}', repl, s)
    s = re.sub(r'\[[^\[\]]+\]', repl, s)
    return s, parts

def unprotect(s, parts):
    for i,p in enumerate(parts):
        s=s.replace(f'§{i}§', p)
    return s

def replace_terms(s, terms):
    for en, tr in sorted(terms.items(), key=lambda kv: len(kv[0]), reverse=True):
        s=s.replace(en, tr)
    return s

jp_phrase = [
    ('Deal X damage to all enemies N times.', 'すべての敵にXダメージをN回与える。'),
    ('Deal X damage to all enemies.', 'すべての敵にXダメージを与える。'),
    ('Deal X damage N times.', 'XダメージをN回与える。'),
    ('Randomly deal X damage N times.', 'ランダムな敵にXダメージをN回与える。'),
    ('Deal X damage.', 'Xダメージを与える。'),
    ('Gain X Block.', 'Xブロックを得る。'),
    ('Gain X energy.', 'Xを得る。'),
    ('Gain X and draw Y cards.', 'Xを得て、カードをY枚引く。'),
    ('Draw X cards.', 'カードをX枚引く。'),
    ('Draw 1 card.', 'カードを1枚引く。'),
    ('Enter Bloom.', '開花に入る。'),
    ('Enter Closed.', '閉合に入る。'),
    ('Switch your current Stance.', '現在のスタンスを切り替える。'),
    ('Apply X Weak.', '脱力Xを付与する。'),
    ('Apply X Vulnerable.', '弱体Xを付与する。'),
    ('Gain X Thorns.', 'トゲXを得る。'),
    ('Gain X Strength.', '筋力Xを得る。'),
    ('Gain X Dexterity.', '敏捷Xを得る。'),
    ('Add X random Danmaku cards to your hand.', 'ランダムな弾幕カードX枚を手札に加える。'),
    ('Randomly play X Unconscious cards from your hand.', '手札から無意識カードをランダムにX枚プレイする。'),
    ('Exhaust this card.', 'このカードを廃棄する。'),
    ('After being played, return it to your hand.', 'プレイ後、このカードを手札に戻す。'),
    ('Can be upgraded any number of times.', '何度でもアップグレードできる。'),
]
kr_phrase = [
    ('Deal X damage to all enemies N times.', '모든 적에게 피해를 X만큼 N번 줍니다.'),
    ('Deal X damage to all enemies.', '모든 적에게 피해를 X 줍니다.'),
    ('Deal X damage N times.', '피해를 X만큼 N번 줍니다.'),
    ('Randomly deal X damage N times.', '무작위 적에게 피해를 X만큼 N번 줍니다.'),
    ('Deal X damage.', '피해를 X 줍니다.'),
    ('Gain X Block.', '방어도를 X 얻습니다.'),
    ('Gain X energy.', 'X를 얻습니다.'),
    ('Gain X and draw Y cards.', 'X를 얻고 카드를 Y장 뽑습니다.'),
    ('Draw X cards.', '카드를 X장 뽑습니다.'),
    ('Draw 1 card.', '카드를 1장 뽑습니다.'),
    ('Enter Bloom.', '개화에 들어갑니다.'),
    ('Enter Closed.', '폐쇄에 들어갑니다.'),
    ('Switch your current Stance.', '현재 태세를 전환합니다.'),
    ('Apply X Weak.', '약화를 X 부여합니다.'),
    ('Apply X Vulnerable.', '취약을 X 부여합니다.'),
    ('Gain X Thorns.', '가시를 X 얻습니다.'),
    ('Gain X Strength.', '힘을 X 얻습니다.'),
    ('Gain X Dexterity.', '민첩을 X 얻습니다.'),
    ('Add X random Danmaku cards to your hand.', '무작위 탄막 카드 X장을 손에 추가합니다.'),
    ('Randomly play X Unconscious cards from your hand.', '손에서 무의식 카드 X장을 무작위로 사용합니다.'),
    ('Exhaust this card.', '이 카드를 소멸시킵니다.'),
    ('After being played, return it to your hand.', '사용한 후 이 카드를 손으로 되돌립니다.'),
    ('Can be upgraded any number of times.', '몇 번이든 강화할 수 있습니다.'),
]

def normalize_template(s):
    # replace protected markers with symbolic slots for phrase matching
    markers=re.findall(r'§\d+§', s)
    t=s
    for idx,m in enumerate(markers):
        t=t.replace(m, 'X' if idx==0 else ('N' if idx==1 else chr(ord('Y')+idx-2)), 1)
    return t, markers

def apply_phrase_templates(s, table):
    lines=[]
    for line in s.split('\n'):
        # split sentences gently while preserving punctuation
        chunks=re.split(r'(?<=\.)\s+', line)
        out=[]
        for ch in chunks:
            t, markers = normalize_template(ch)
            done=False
            for pat, repl in table:
                if t == pat:
                    r=repl
                    for idx,m in enumerate(markers):
                        sym='X' if idx==0 else ('N' if idx==1 else chr(ord('Y')+idx-2))
                        r=r.replace(sym,m)
                    out.append(r); done=True; break
            if not done:
                out.append(ch)
        lines.append(' '.join(out))
    return '\n'.join(lines)

# Additional full-string translations for recurring custom prose/dialog.
jp_exact = {
    'Choose 1 card to put into your hand': '手札に加えるカードを1枚選ぶ',
    'Choose a card to gain Unconscious.': '無意識を得るカードを1枚選ぶ。',
    'Continue': '続ける', 'Accept': '受け取る', 'Answer': '答える',
    'Rose': '薔薇', 'Miracle Shrine Maiden': '奇跡の巫女', 'Lamprey Grill Stand': '八目鰻の屋台',
    'Talk to her.': '話しかける。', 'Ignore her.': '無視する。', 'Accept her blessing.': '祝福を受ける。',
    'Accept the omamori.': 'お守りを受け取る。', 'Watch the Moriya Dance.': '守矢大舞を見る。',
    'Just a rice ball.': 'おにぎりだけ。', 'Order your favorite set meal.': '好きな定食を頼む。',
    'Order an Oedo Boat Festival and Fourteenth Night!': '大江戸船祭と十四夜を頼む！', 'Eat the proprietress!': '女将を食べる！',
    'Not enough Gold.': 'ゴールドが足りない。', 'You cannot afford this meal.': 'この食事を注文する余裕がない。',
    'Reimu Hakurei': '博麗霊夢', 'Shrine Maiden of Paradise': '楽園の素敵な巫女',
    'Kanako': 'Kanako', 'Suwako': 'Suwako',
}
kr_exact = {
    'Choose 1 card to put into your hand': '손에 넣을 카드 1장을 선택합니다',
    'Choose a card to gain Unconscious.': '무의식을 얻을 카드 1장을 선택합니다.',
    'Continue': '계속', 'Accept': '받기', 'Answer': '대답',
    'Rose': '장미', 'Miracle Shrine Maiden': '기적의 무녀', 'Lamprey Grill Stand': '칠성장어 구이 노점',
    'Talk to her.': '말을 건다.', 'Ignore her.': '무시한다.', 'Accept her blessing.': '축복을 받는다.',
    'Accept the omamori.': '부적을 받는다.', 'Watch the Moriya Dance.': '모리야 대무를 본다.',
    'Just a rice ball.': '주먹밥만.', 'Order your favorite set meal.': '좋아하는 정식을 주문한다.',
    'Order an Oedo Boat Festival and Fourteenth Night!': '오에도 선박 축제와 열넷째 밤을 주문한다!', 'Eat the proprietress!': '여주인을 먹는다!',
    'Not enough Gold.': '골드가 부족합니다.', 'You cannot afford this meal.': '이 식사를 살 수 없습니다.',
    'Reimu Hakurei': '하쿠레이 레이무', 'Shrine Maiden of Paradise': '낙원의 멋진 무녀',
    'Kanako': 'Kanako', 'Suwako': 'Suwako',
}

def rough_jp_sentence(s):
    s, parts = protect(s)
    if s in jp_exact:
        return jp_exact[s]
    s = replace_terms(s, jp_terms)
    s = apply_phrase_templates(s, jp_phrase)
    # broader prose/dialog substitutions
    repls = [
        ('Whenever you ', 'あなたが'), ('When you ', 'あなたが'), ('At the start of your turn', 'あなたのターン開始時'), ('At end of turn', 'ターン終了時'),
        ('For each ', ''), ('for each ', 'につき'), ('from your hand', '手札から'), ('in your hand', '手札にある'), ('to your hand', '手札に'),
        ('draw pile', '山札'), ('discard pile', '捨て札'), ('exhaust pile', '廃棄札'), ('all enemies', 'すべての敵'),
        ('random ', 'ランダムな'), ('Randomly ', 'ランダムに'), ('automatically play', '自動的にプレイする'), ('play ', 'プレイする'),
        ('Generate ', '生成する'), ('generate ', '生成する'), ('Add ', '加える'), ('add ', '加える'), ('Choose ', '選ぶ'), ('choose ', '選ぶ'),
        ('Upgrade ', 'アップグレードする'), ('upgrade ', 'アップグレードする'), ('Heal ', 'HPを回復する'), ('Spend ', '支払う'), ('Obtain ', '入手する'),
        ('Take ', '受ける'), ('damage', 'ダメージ'), ('card(s)', '枚のカード'), ('cards', 'カード'), ('card', 'カード'), ('this combat', 'この戦闘中'),
        ('this turn', 'このターン'), ('costs ', 'コストが'), ('less', '減少する'), ('more', '増加する'), ('additional', '追加の'),
        ('If ', 'もし'), ('Otherwise, ', 'そうでなければ、'), ('up to ', '最大'), ('from ', 'から'), ('into ', 'に'), (' and ', 'し、'), (' with ', 'で'),
        ('You ', 'あなたは'), ('your ', 'あなたの'), ('the ', ''), ('a ', ''), ('an ', ''),
    ]
    for a,b in repls: s=s.replace(a,b)
    s=s.replace('.', '。').replace('?', '？').replace('!', '！')
    return unprotect(s, parts)

def rough_kr_sentence(s):
    s, parts = protect(s)
    if s in kr_exact:
        return kr_exact[s]
    s = replace_terms(s, kr_terms)
    s = apply_phrase_templates(s, kr_phrase)
    repls = [
        ('Whenever you ', '당신이 '), ('When you ', '당신이 '), ('At the start of your turn', '내 턴 시작 시'), ('At end of turn', '턴 종료 시'),
        ('from your hand', '손에서'), ('in your hand', '손에 있는'), ('to your hand', '손에'), ('draw pile', '뽑을 카드 더미'),
        ('discard pile', '버린 카드 더미'), ('exhaust pile', '소멸된 카드 더미'), ('all enemies', '모든 적'),
        ('random ', '무작위 '), ('Randomly ', '무작위로 '), ('automatically play', '자동으로 사용합니다'), ('play ', '사용합니다 '),
        ('Generate ', '생성합니다 '), ('generate ', '생성합니다 '), ('Add ', '추가합니다 '), ('add ', '추가합니다 '), ('Choose ', '선택합니다 '),
        ('Upgrade ', '강화합니다 '), ('upgrade ', '강화합니다 '), ('Heal ', '회복합니다 '), ('Spend ', '소모합니다 '), ('Obtain ', '획득합니다 '),
        ('Take ', '받습니다 '), ('damage', '피해'), ('card(s)', '장의 카드'), ('cards', '카드'), ('card', '카드'), ('this combat', '이번 전투 동안'),
        ('this turn', '이번 턴'), ('costs ', '비용이 '), ('less', '감소합니다'), ('more', '증가합니다'), ('additional', '추가'),
        ('If ', '만약 '), ('Otherwise, ', '그렇지 않으면 '), ('up to ', '최대 '), ('from ', '에서 '), ('into ', '에 '), (' and ', ' 그리고 '),
        (' with ', '으로 '), ('You ', '당신은 '), ('your ', '당신의 '), ('the ', ''), ('a ', ''), ('an ', ''),
    ]
    for a,b in repls: s=s.replace(a,b)
    s=s.replace('.', '.').replace('?', '?').replace('!', '!')
    return unprotect(s, parts)

def should_keep_title(key, val):
    # User preference: card/relic names can stay as file/English names. Keep all titles except explicit event/ancient UI exact maps.
    return key.endswith('.title') and not ('EVENT' in key)

def translate_value(key, val, lang):
    if not isinstance(val, str): return val
    exact = jp_exact if lang=='jpn' else kr_exact
    trans = rough_jp_sentence if lang=='jpn' else rough_kr_sentence
    if val in exact:
        return exact[val]
    if key.endswith('.speaker'):
        return val
    if key.endswith('.title'):
        # keep most card/relic/power names in English; translate event/ancient titles when exact known
        return exact.get(val, val)
    # Preserve very short proper names unless exact translated
    if len(val) < 18 and not any(ch in val for ch in '.!?\n') and key.endswith('.epithet') is False:
        return exact.get(val, val)
    return trans(val)

for lang in ['jpn','kor']:
    outdir=root/lang
    outdir.mkdir(exist_ok=True)
    for src in sorted(eng_dir.glob('*.json')):
        data=json.loads(src.read_text(encoding='utf-8-sig'))
        out={k: translate_value(k,v,lang) for k,v in data.items()}
        (outdir/src.name).write_text(json.dumps(out, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
print('done')
